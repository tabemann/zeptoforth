#!/usr/bin/env python3

# Copyright (c) 2020 Travis Bemann
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
import re

def parse_devices(memmap):
    devices = []
    regex = re.compile(r'^(defined\? use-)([A-Za-z0-9_]*)(.*)$')
    line = memmap.readline()
    while line:
        match = regex.match(line)
        if match:
            devices.append(match.group(2))
        line = memmap.readline()
    return devices

def get_device(devices, register):
    for device in devices:
        if register[0:len(device)] == device:
            return device
    return None

def process(memmap, bitfields, output):
    head_regex = re.compile(r'^(\\\s)([A-Za-z0-9_]*?)(\s\(.*)$')
    line_regex = re.compile(r'^:\s.*$')
    devices = parse_devices(memmap)
    line = bitfields.readline()
    while line:
        match = head_regex.match(line)
        if match:
            device = get_device(devices, match.group(2))
            if device:
                output.write('defined? use-%s defined? %s not and [if]\n' %
                             (device, device))
                output.write(line)
                line = bitfields.readline()
                while line:
                    match = line_regex.match(line)
                    if not match:
                        break
                    output.write(line)
                    line = bitfields.readline()
                output.write('[then]\n')
                output.write(line)
        else:
            output.write(line)
        line = bitfields.readline()
    
def main():
    if len(sys.argv) != 4:
        print('Usage: %s <memmap> <bitfields> <output>')
        sys.exit(1)
    else:
        memmap = open(sys.argv[1], 'r')
        bitfields = open(sys.argv[2], 'r')
        output = open(sys.argv[3], 'w')
        process(memmap, bitfields, output)

if __name__ == '__main__':
    main()
