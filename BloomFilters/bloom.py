# -*- encoding: utf-8 -*-
"""This module implements a bloom filter probabilistic data structure.

"""
from __future__ import absolute_import
import math
import hashlib
import base64
import json
from struct import unpack, calcsize

try:
    import bitarray
except ImportError:
    raise ImportError('pybloom requires bitarray >= 0.3.4')

__version__ = '2.0m'
__author__ = "Jay Baird <jay.baird@me.com>, Bob Ippolito <bob@redivi.com>,\
               Marius Eriksen <marius@monkey.org>,\
               Alex Brasetvik <alex@brasetvik.com>,\
               Matt Bachmann <bachmann.matt@gmail.com>,\
              "
__modified__ = " Azhar Nisar azhar.nisar@nhs.net , modified to support portability, \
                main changes in toFile and fromFile method to use json instead of native \
                file format, also changed salting to use single string instead of hash of hash"

def make_hashfuncs(num_slices, num_bits):
    if num_bits >= (1 << 31):
        fmt_code, chunk_size = 'Q', 8
    elif num_bits >= (1 << 15):
        fmt_code, chunk_size = 'I', 4
    else:
        fmt_code, chunk_size = 'H', 2

    total_hash_bits = 8 * num_slices * chunk_size
    if total_hash_bits > 384:
        hashfn = hashlib.sha512
    elif total_hash_bits > 256:
        hashfn = hashlib.sha384
    elif total_hash_bits > 160:
        hashfn = hashlib.sha256
    elif total_hash_bits > 128:
        hashfn = hashlib.sha1
    else:
        hashfn = hashlib.md5

    fmt = fmt_code * (hashfn().digest_size // chunk_size)
    num_salts, extra = divmod(num_slices, len(fmt))
    if extra:
        num_salts += 1


    salts = tuple(hashfn(hashfn(str(i)).digest()) for i in xrange(num_salts))

    def _make_hashfuncs(key):
        if isinstance(key, unicode):
            key = key.encode('utf-8')
        else:
            key = str(key)
        i = 0
        for salt in salts:
            h = salt.copy()
            h.update(key)
            for uint in unpack(fmt, h.digest()):
                yield uint % num_bits
                i += 1
                if i >= num_slices:
                    return

    return _make_hashfuncs


class BloomFilter(object):

    def __init__(self, capacity, error_rate=0.001):
        """Implements a space-efficient probabilistic data structure
        capacity
            this BloomFilter must be able to store at least *capacity* elements
            while maintaining no more than *error_rate* chance of false
            positives
        error_rate
            the error_rate of the filter returning false positives. This
            determines the filters capacity. Inserting more than capacity
            elements greatly increases the chance of false positives.
        >>> b = BloomFilter(capacity=100000, error_rate=0.001)
        >>> b.add("test")
        False
        >>> "test" in b
        True
        """
        if not (0 < error_rate < 1):
            raise ValueError("Error_Rate must be between 0 and 1.")
        if not capacity > 0:
            raise ValueError("Capacity must be > 0")
        # given M = num_bits, k = num_slices, P = error_rate, n = capacity
        #       k = log2(1/P)
        # solving for m = bits_per_slice
        # n ~= M * ((ln(2) ** 2) / abs(ln(P)))
        # n ~= (k * m) * ((ln(2) ** 2) / abs(ln(P)))
        # m ~= n * abs(ln(P)) / (k * (ln(2) ** 2))
        num_slices = int(math.ceil(math.log(1.0 / error_rate, 2)))
        bits_per_slice = int(math.ceil(
            (capacity * abs(math.log(error_rate))) /
            (num_slices * (math.log(2) ** 2))))
        self._setup(error_rate, num_slices, bits_per_slice, capacity, 0)
        self.bitarray = bitarray.bitarray(self.num_bits, endian='little')
        self.bitarray.setall(False)


    def _setup(self, error_rate, num_slices, bits_per_slice, capacity, count):
        self.error_rate = error_rate
        self.num_slices = num_slices
        self.bits_per_slice = bits_per_slice
        self.capacity = capacity
        self.num_bits = num_slices * bits_per_slice
        self.count = count
        self.make_hashes = make_hashfuncs(self.num_slices, self.bits_per_slice)

    def __contains__(self, key):
        """Tests a key's membership in this bloom filter.
        >>> b = BloomFilter(capacity=100)
        >>> b.add("hello")
        False
        >>> "hello" in b
        True
        """
        bits_per_slice = self.bits_per_slice
        bitarray = self.bitarray
        hashes = self.make_hashes(key)
        offset = 0
        for k in hashes:
            if not bitarray[offset + k]:
                return False
            offset += bits_per_slice
        return True

    def __len__(self):
        """Return the number of keys stored by this bloom filter."""
        return self.count

    def add(self, key, skip_check=False):
        """ Adds a key to this bloom filter. If the key already exists in this
        filter it will return True. Otherwise False.
        >>> b = BloomFilter(capacity=100)
        >>> b.add("hello")
        False
        >>> b.add("hello")
        True
        >>> b.count
        1
        """
        bitarray = self.bitarray
        bits_per_slice = self.bits_per_slice
        hashes = self.make_hashes(key)
        found_all_bits = True
        if self.count > self.capacity:
            raise IndexError("BloomFilter is at capacity")
        offset = 0
        for k in hashes:
            if not skip_check and found_all_bits and not bitarray[offset + k]:
                found_all_bits = False
            self.bitarray[offset + k] = True
            offset += bits_per_slice

        if skip_check:
            self.count += 1
            return False
        elif not found_all_bits:
            self.count += 1
            return False
        else:
            return True

    def copy(self):
        """Return a copy of this bloom filter.
        """
        new_filter = BloomFilter(self.capacity, self.error_rate)
        new_filter.bitarray = self.bitarray.copy()
        return new_filter

    def union(self, other):
        """ Calculates the union of the two underlying bitarrays and returns
        a new bloom filter object."""
        if self.capacity != other.capacity or \
            self.error_rate != other.error_rate:
            raise ValueError("Unioning filters requires both filters to have \
both the same capacity and error rate")
        new_bloom = self.copy()
        new_bloom.bitarray = new_bloom.bitarray | other.bitarray
        return new_bloom

    def __or__(self, other):
        return self.union(other)

    def intersection(self, other):
        """ Calculates the intersection of the two underlying bitarrays and returns
        a new bloom filter object."""
        if self.capacity != other.capacity or \
            self.error_rate != other.error_rate:
            raise ValueError("Intersecting filters requires both filters to \
have equal capacity and error rate")
        new_bloom = self.copy()
        new_bloom.bitarray = new_bloom.bitarray & other.bitarray
        return new_bloom

    def __and__(self, other):
        return self.intersection(other)

    def tofile(self, f):
        """Write the bloom filter to file object `f'. Underlying bits
        are written as machine values. This is much more space
        efficient than pickling the object."""

        jsonDict = {}
        jsonDict['error_rate'] = self.error_rate
        jsonDict['num_slices'] = self.num_slices
        jsonDict['bits_per_slice'] = self.bits_per_slice
        jsonDict['capacity'] = self.capacity
        jsonDict['count'] = self.count
        jsonDict['bits'] = base64.b64encode(self.bitarray.tobytes())

        f.write(json.dumps(jsonDict))

    @classmethod
    def fromfile(cls, f):
        """Read a bloom filter from file-object `f' serialized with
        ``BloomFilter.tofile''."""
        jsonString = f.read()
        jsonDict = json.loads(jsonString)


        base64DecodeBits = base64.b64decode(jsonDict['bits'])
        filter = cls(1)  # Bogus instantiation, we will `_setup'.
        filter._setup(jsonDict['error_rate'], jsonDict['num_slices'],
                      jsonDict['bits_per_slice'], jsonDict['capacity'],
                      jsonDict['count'])
        filter.bitarray = bitarray.bitarray(endian='little')
        filter.bitarray.frombytes(base64DecodeBits)

        if filter.num_bits != filter.bitarray.length() and \
               (filter.num_bits + (8 - filter.num_bits % 8)
                != filter.bitarray.length()):
            raise ValueError('Bit length mismatch!')

        return filter

    def __getstate__(self):
        d = self.__dict__.copy()
        del d['make_hashes']
        return d

    def __setstate__(self, d):
        self.__dict__.update(d)
        self.make_hashes = make_hashfuncs(self.num_slices, self.bits_per_slice)


if __name__ == "__main__":
    import doctest
    doctest.testmod()
