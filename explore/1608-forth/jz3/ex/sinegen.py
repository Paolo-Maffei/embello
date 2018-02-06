#!/usr/bin/env python
# generate a 12-bit sinewave table for use in Forth

import math
import textwrap

N = 500  # entries
M = 3800  # max value, slight less than full 12-bit range
O = 2048  # offset

values = []

for i in range(N):
    f = math.sin(2*math.pi/N*i)
    i = str(int(round(f / 2 * M + O)))
    values.append(i)

code = " h, ".join(values) + " h,"
wrapped = textwrap.wrap(code, 77)

print "compiletoflash"
print "create SINE{}".format(N)
print "  " + "\n  ".join(wrapped)
print "\\ end sine"
