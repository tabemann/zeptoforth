#!/usr/bin/env python3

# Copyright (c) 2023 Travis Bemann
#
# Permission is hereby granted, free of charge, to any person obtaining a copy
# of this software and associated documentation files (the "Software"), to deal
# in the Software without restriction, including without limitation the rights
# to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the Software is
# furnished to do so, subject to the following conditions:
# 
# The above copyright notice and this permission notice shall be included in
# all copies or substantial portions of the Software.
# 
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
# OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
# SOFTWARE.

import sys
import struct

def main():
    if len(sys.argv) == 4:
        with open(sys.argv[2], 'rb') as in_file:
            data = in_file.read()
            in_file.close()
            with open(sys.argv[3], 'w') as out_file:
                out_file.write('compile-to-flash\n\n')
                out_file.write('begin-module %s\n\n' % sys.argv[1])
                out_file.write('  create data\n')
                index = 0
                for offset in range(0, len(data), 4):
                    if offset <= len(data) - 4:
                        (value,) = struct.unpack('<L', data[offset : offset+4])
                        if index % 4 == 0:
                            out_file.write('\n ')
                        index += 1
                        out_file.write(' $%08X ,' % value)
                if len(data) % 4 != 0:
                    if index % 4 == 0:
                        out_file.write('\n ')
                    for byte in data[len(data) - (len(data) % 4) : len(data)]:
                        out_file.write(' $%02X c,' % byte)
                out_file.write('\n\n  here data - constant size\n\n')
                out_file.write('end-module\n\n')
                out_file.write('compile-to-ram\n')
                out_file.close()
                        
    else:
        sys.exit('Usage: %s <name> <in-binary> <out-forth>' % sys.argv[0])

if __name__ == '__main__':
    main()
