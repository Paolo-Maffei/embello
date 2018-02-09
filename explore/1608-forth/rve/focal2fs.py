#!/usr/bin/env python
# convert FOCAL binary for use in Forth

import struct
import textwrap

with open('focal.rom', 'rb') as f:
    data = f.read()

values = struct.unpack('<4096h', data)
code = " h, ".join(map(str, values)) + " h,"
wrapped = textwrap.wrap(code, 77)

print "compiletoflash"
print "create ROM"
print "  " + "\n  ".join(wrapped)
print "8192 constant ROM-SIZE"
print "\\ end rom"
