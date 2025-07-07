#!/usr/bin/env python3

# The MIT License 
#  
# Copyright (c) 2020 TG9541 
# Copyright (c) 2020-2024 Travis Bemann
#  
# Permission is hereby granted, free of charge,  
# to any person obtaining a copy of this software and  
# associated documentation files (the "Software"), to  
# deal in the Software without restriction, including  
# without limitation the rights to use, copy, modify,  
# merge, publish, distribute, sublicense, and/or sell  
# copies of the Software, and to permit persons to whom  
# the Software is furnished to do so,  
# subject to the following conditions: 
#  
# The above copyright notice and this permission notice  
# shall be included in all copies or substantial portions of the Software. 
#  
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,  
# EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES  
# OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  
# IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR  
# ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,  
# TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE  
# SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

# STM8EF code loader, experimental version for Python3
# targets serial interface, and text file
# supports e4thcom pseudo words, e.g. #include, #require, and \res .
#
# The include path is:
#   1. path of the included file (in extension to e4thcom)
#   2. ./
#   3. ./mcu
#   4. ./target, or <args.base>/target (-b option)
#   5. ./lib

import sys
import os
import serial
import re
import argparse
import time

# Whether to disable the timeout
disableTimeout = False

# hash for "\res MCU:" key-value pairs
resources = {}

# a modicum of OOP for target line transfer
class Connection:
    def dotrans(self, line):
        return "ok"
    def transfer(self, line):
        return self.dotrans(line)
    def testtrans(self, line):
        return self.dotrans(line)
    def tracef(self, line):
        if (tracefile):
            try:
                tracefile.write(line + '\n')
            except:
                print('Error writing tracefile %s' % args.tracefile)
                exit(1)

# dummy line transfer
class ConnectDryRun(Connection):
    def transfer(self, line):
        print(line)
        return "ok"
    def testtrans(self, line):
        return ""

# serial line transfer
class ConnectSerial(Connection):
    port = { }
    def __init__(self, ttydev, baud):
        self.ttydev = ttydev
        self.baud = baud
        try:
            self.port = serial.Serial(
                port     = ttydev,
                baudrate = baud,
                parity   = serial.PARITY_NONE,
                stopbits = serial.STOPBITS_ONE,
                bytesize = serial.EIGHTBITS,
                timeout  = None if disableTimeout else 5 )
        except:
            print('Error: TTY device %s invalid' % ttydev)
            sys.exit(1)

    def transfer(self, line):
        vprint('TX: ' + line)
        return self.dotrans(line)

    def dotrans(self, line):
        self.tracef(line)

        try:
            line = removeComment(line)
            if (len(line) > 0):
                self.port.write(str.encode(line + '\r'))
                sioResult = self.port.readline().decode()
            else:
                return "ok"
        except:
            if (re.search('reboot', line)):
                self.port.close()
                time.sleep(2)
                try:
                    self.port = serial.Serial(
                        port     = self.ttydev,
                        baudrate = self.baud,
                        parity   = serial.PARITY_NONE,
                        stopbits = serial.STOPBITS_ONE,
                        bytesize = serial.EIGHTBITS,
                        timeout  = None if disableTimeout else 5 )
                    self.refresh()
                except:
                    print('Error: TTY device %s invalid' % ttydev)
                    sys.exit(1)
                return "ok"
#            elif (re.search('warm', line)):
#                time.sleep(2)
            else:
                print('Error: TTY transmission failed')
                sys.exit(1)

        if (re.search(' (OK|ok)\r\n$', sioResult)):
            return "ok"
        elif (re.search('reboot', sioResult)):
            self.refresh()
            return "ok"
        elif (re.search('warm', sioResult)):
            time.sleep(2)
            self.port.flush()
            self.port.reset_output_buffer()
            self.port.reset_input_buffer()
            return "ok"
        else:
            return sioResult

    def refresh(self):
        time.sleep(0.5)
        self.port.flush()
        self.port.reset_output_buffer()
        self.port.reset_input_buffer()
        time.sleep(0.5)
        self.port.flush()
        self.port.reset_output_buffer()
        self.port.reset_input_buffer()
        
# simple show-error-and-exit
def error(message, line, path, lineNr):
    print('Error file %s line %d: %s' % (path, lineNr, message))
    print('>>>  %s' % (line))
    sys.exit(1)

# simple stdout log printer
def vprint(text):
    if (args.verbose):
        print(text)

# search an item (a source file) in the extended e4thcom search path
def searchItem(item, CPATH):
    # Windows' DOS days quirks: hack for STM8EF subfolders in lib/
    if (os.name == 'nt' and re.search('^(hw|utils|math)',item)):
        item = item.replace('/','\\',1)

    CWDPATH = os.getcwd()

    # def.1: folder of current item
    searchRes = os.path.join(CPATH, item)
    if (not os.path.isfile(searchRes)):
        # 2: ./ (e4thcom: cwd)
        searchRes = os.path.join(CWDPATH, item)
    if (not os.path.isfile(searchRes)):
        # 3: ./mcu (e4thcom: cwd/mcu)
        searchRes = os.path.join(CWDPATH, 'mcu', item)
    if (not os.path.isfile(searchRes)):
        # 4: ./target (e4thcom: cwd/target), or <args.base>/target (-b option)
        searchRes = os.path.join(CWDPATH, args.base,'target', item)
    if (not os.path.isfile(searchRes)):
        # 5: ./lib (e4thcom: cwd/lib)
        searchRes = os.path.join(CWDPATH, 'lib', item)
    if (not os.path.isfile(searchRes)):
        searchRes = ''
    return searchRes

# Forth "\" comment stripper
def removeComment(line):
    if (re.search('^\\\\ +', line)):
        return ''
    elif (re.search('^\\\\$', line)):
        return ''
    else:
        return line.partition(' \\ ')[0].strip()

# test if a word already exists in the dictionary
def required(word):
    return CN.testtrans("' %s DROP" % word) != 'ok'

# reader for e4thcom style .efr files (symbol-address value pairs)
def readEfr(path):
    with open(path) as source:
        vprint('Reading efr file %s' % path)
        lineNr = 0
        try:
            CPATH = os.path.dirname(path)
            for line in source.readlines():
                lineNr += 1
                line = removeComment(line)
                if (not line):
                    continue
                resItem = line.rsplit()
                if (resItem[1] == 'equ'):
                    resources[resItem[2]] = resItem[0]

        except ValueError as err:
            print(err.args[0])
            exit(1)

# uploader with resolution of #include, #require, and \res
def upload(path):

    reSkipToEOF = re.compile("^\\\\\\\\")

    with open(path) as source:
        vprint('Uploading %s' % path)
        lineNr = 0
        commentEOF = False
        commentBlock = False

        try:
            CPATH = os.path.dirname(path)
            for line in source.readlines():
                lineNr += 1
                line = line.replace('\n', ' ').replace('\r', '').strip()

                # all lines from "\\ Example:" on are comments
                if (reSkipToEOF.match(line)):
                    commentEOF = True

                # e4thcom style block comments (may not end in SkipToEOF section)
                if (commentEOF or commentBlock):
                    vprint('\\ ' + line)
                    continue

                if (re.search('^\\\\index ', line)):
                    continue

                if (re.search('^\\\\res ', line)):
                    resSplit = line.rsplit()
                    if (resSplit[1] == 'MCU:'):
                        mcuFile = resSplit[2].strip()
                        if (not re.search('\\.efr', mcuFile)):
                            mcuFile = mcuFile + '.efr'
                        mcuFile = searchItem(mcuFile,CPATH)
                        if (not mcuFile):
                            error('file not found', line, path, lineNr)
                        else:
                            readEfr(mcuFile)
                    elif (resSplit[1] == 'export'):
                        for i in range(2, len(resSplit)):
                            symbol = resSplit[i]
                            if (not symbol in resources):
                                error('symbol not found: %s' % symbol, line, path, lineNr)
                            if (required(symbol)):
                                CN.transfer("$%s CONSTANT %s" % (resources[symbol], symbol))
                            else:
                                vprint("\\res export %s: skipped" % symbol)
                    continue

                reInclude = re.search('^#(include|require) +(.+?)$', line)
                if (reInclude):
                    includeMode = reInclude.group(1).strip()
                    includeItem = reInclude.group(2).strip()

                    if (includeMode == 'require' and not required(includeItem)):
                        vprint("#require %s: skipped" % includeItem)
                        continue

                    includeFile = searchItem(includeItem,CPATH)
                    if (includeFile == ''):
                        error('file not found', line, path, lineNr)
                    try:
                        upload(includeFile)
                        if (includeMode == 'require' and required(includeItem)):
                            result = CN.transfer(": %s ;" % includeItem)
                            if (result != 'ok'):
                                raise ValueError('error closing #require %s' % result)
                    except:
                        error('could not upload file', line, path, lineNr)
                    continue
                if (len(line) > 255):
                    raise ValueError('Line is too long: %s' % (line))

                result = CN.transfer(line)
                if (result != 'ok'):
                    raise ValueError('error %s' % result)

        except ValueError as err:
            print(err.args[0])
            exit(1)

# Python has a decent command line argument parser - use it
parser = argparse.ArgumentParser()
parser.add_argument("method", choices=['serial','dryrun'],
        help="transfer method")
parser.add_argument("files", nargs='*',
        help="name of one or more files to transfer")
parser.add_argument("-b", "--target-base", dest="base", default="",
        help="target base folder, default: ./", metavar="base")
parser.add_argument("-p", "--port", dest="port",
        help="PORT for transfer, default: /dev/ttyUSB0", metavar="port")
parser.add_argument("-q", "--quiet", action="store_false", dest="verbose", default=True,
        help="don't print status messages to stdout")
parser.add_argument("-t", "--trace", dest="tracefile",
        help="write source code (with includes) to tracefile", metavar="tracefile")
parser.add_argument("-B", "--baud", dest="baud", type=int,
                    help="baud rate, default 9600", metavar="baud")
args = parser.parse_args()

# create tracefile if needed
if (args.tracefile):
    try:
        tracefile = open(args.tracefile,'w')
    except:
        print('Error writing tracefile %s' % args.tracefile)
        exit(1)
else:
    tracefile = False

# Initalize transfer method with default destionation port
if (args.method == "serial"):
    CN = ConnectSerial(args.port or '/dev/ttyUSB0', args.baud or 9600)
    CN.refresh()
else:
    CN = ConnectDryRun()

# Disable the timeout if extra/common/setup_blocks_fat32.fs is being uploaded
if 'extra/common/setup_blocks_fat32.fs' in args.files:
    disableTimeout = True
    
# Ze main
for path in args.files:
    upload(path)
