"""make_float64_test.py: writes float64_test.fs, double precision against IEEE doubles by their bits, from the cases below: the expected values are Python's. The arithmetic must match
exactly, and the maths functions come from another library, so they may differ by 1 unit in the last place. The file checks itself and needs nothing but a Forth with these words:
include float64_test.fs, and it ends by saying how many tests failed.

    python test/fp/make_float64_test.py
"""
import math, os, struct
HERE = os.path.dirname(os.path.abspath(__file__))
LICENCE = '''Copyright (c) 2026 Peter Mather

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
'''


def licensed(text):
    return ''.join((chr(92) + ' ' + l).rstrip() + '\n' for l in LICENCE.split('\n')[:-1]) + '\n' + text


pi = math.pi
CASES = [  # (forth, expected, exact?)
    ('1de0 3de0 dv/', 1 / 3, True), ('2de0 dvsqrt', math.sqrt(2), True), ('1.5de0 2.25de0 dv+', 3.75, True), ('1de0 1de-16 dv+', 1 + 1e-16, True),
    ('0.1de0 0.2de0 dv+', 0.1 + 0.2, True), ('1de308 10de0 dv/', 1e307, True), ('123456789de0 987654321de0 dv*', 123456789.0 * 987654321.0, True),
    ('5de-324 2de0 dv*', 5e-324 * 2, True), ('1.7976931348623157de308', 1.7976931348623157e308, True), ('2.2250738585072014de-308', 2.2250738585072014e-308, True),
    ('7de0 dvnegate', -7.0, True), ('-7de0 dvabs', 7.0, True), ('2147483647 n>dv', 2147483647.0, True), ('-2147483648 n>dv', -2147483648.0, True), ('$FFFFFFFF u>dv', 4294967295.0, True),
    ('2.75de0 dvfloor', 2.0, True), ('-2.75de0 dvfloor', -3.0, True), ('2.25de0 dvceil', 3.0, True), ('-2.25de0 dvceil', -2.0, True), ('2.75de0 dvnfract', 2.0, True), ('-2.75de0 dvnfract', -2.0, True),
    ('2.75de0 dvfract', 0.75, True), ('2.5de0 dvround-half-even', 2.0, True), ('3.5de0 dvround-half-even', 4.0, True), ('2.5de0 dvround-half-away-zero', 3.0, True), ('-2.5de0 dvround-half-away-zero', -3.0, True),
    ('2.5de0 dvround-half-up', 3.0, True), ('2.5de0 dvround-half-down', 2.0, True), ('2.5de0 dvround-half-zero', 2.0, True), ('2.5de0 dvround-half-odd', 3.0, True), ('3.5de0 dvround-half-odd', 3.0, True),
    ('2.1de0 dvround-away-zero', 3.0, True), ('-2.1de0 dvround-away-zero', -3.0, True), ('2.9de0 dvround-zero', 2.0, True), ('1de0 5de0 dvmin', 1.0, True), ('1de0 5de0 dvmax', 5.0, True),
    ('dv1', 1.0, True), ('dv-1', -1.0, True), ('dvpi', pi, True), ('7de0 4de0 dvmod', -1.0, True), ('1,5 f64>dv', 1.5, True), ('-2,25 f64>dv', -2.25, True), ('$40490FDB v>dv', struct.unpack('<f', struct.pack('<I', 0x40490FDB))[0], True),
    ('dvpi 6de0 dv/ dvsin', math.sin(pi / 6), False), ('1de0 dvcos', math.cos(1), False), ('1de0 dvtan', math.tan(1), False), ('0.5de0 dvasin', math.asin(.5), False), ('0.5de0 dvacos', math.acos(.5), False), ('1de0 dvatan', math.atan(1), False),
    ('3de0 4de0 dvatan2', math.atan2(3, 4), False), ('1de0 dvexp', math.e, False), ('10de0 dvln', math.log(10), False), ('1de-10 dvexpm1', math.expm1(1e-10), False), ('1de-10 dvlnp1', math.log1p(1e-10), False),
    ('1de0 dvsinh', math.sinh(1), False), ('1de0 dvcosh', math.cosh(1), False), ('1de0 dvtanh', math.tanh(1), False), ('1de0 dvasinh', math.asinh(1), False), ('2de0 dvacosh', math.acosh(2), False), ('0.5de0 dvatanh', math.atanh(.5), False),
    ('2de0 10de0 dv**', 1024.0, False), ('2de0 0.5de0 dv**', math.sqrt(2), False), ('2de0 10 dvi**', 1024.0, False), ('2de0 -1de0 dv**', 0.5, False),
]
FLAGS = [  # (forth leaving a flag or cell, expected)
    ('1de0 2de0 dv<', -1), ('2de0 1de0 dv<', 0), ('1de0 1de0 dv<=', -1), ('1de0 1de0 dv=', -1), ('1de0 2de0 dv=', 0), ('1de0 2de0 dv<>', -1), ('2de0 1de0 dv>', -1), ('1de0 1de0 dv>=', -1),
    ('-1de0 dv0<', -1), ('0de0 dv0=', -1), ('1de0 dv0>', -1), ('0de0 dv0>=', -1), ('0de0 dv0<=', -1), ('1de0 dv0<>', -1), ('7 n>dv 2de0 dv/ dv>n', 3), ('-7 n>dv 2de0 dv/ dv>n', -3), ('3de9 dv>u', 3000000000 - 2**32),
    ('1de300 1de300 dv* dv>infinite?', -1), ('-1de0 dvsqrt dv>nan?', -1), ('1de0 dv>nan?', 0), ('1de0 dv>infinite?', 0), ('-1de0 dvsqrt 1de0 dv<', 0), ('-1de0 dvsqrt 1de0 dv>', 0), ('-1de0 dvsqrt -1de0 dvsqrt dv=', 0), ('-1de0 dvsqrt -1de0 dvsqrt dv<>', -1),
]
LITERALS = [  # (forth, expected): how a literal may be written
    ('1.5DE0', 1.5), ('1.5dE0', 1.5), ('1.5De0', 1.5), ('-2.25de-3', -2.25e-3), ('.5de0', .5), ('5.de0', 5.0), ('1de+3', 1000.0), ('2de3', 2000.0),
]
TEXT = [  # (forth, expected): what format-dv makes of a value, which is this port's: C's %.16g
    ('0.1de0', '0.1'), ('1de100', '1e+100'), ('-2.5de0', '-2.5'), ('1de0 3de0 dv/', '0.3333333333333333'), ('dvpi', '3.141592653589793'),
    ('k2', '1.5625'), ('twofive', '2.5'), ('dvar 2@', '6.5'),
]
FIXED = [  # (forth, lo, hi): to S31.32
    ('2.25de0 dv>f64', 0x40000000, 2), ('-2.25de0 dv>f64', 0xC0000000, -3 & 0xFFFFFFFF),
]


def bits(x):
    b = struct.unpack('<Q', struct.pack('<d', x))[0]
    return '$%08X $%08X' % (b & 0xFFFFFFFF, b >> 32)


f = ['\\ float64_test.fs: double precision against IEEE doubles, by their bits. Written by make_float64_test.py, which has the cases: change them there.',
     '\\ It is for a float64 module with these words, which this tree does not have yet. The one it was written against is forth/float64.fs of https://github.com/UKTailwind/CMM2V1.5/tree/main/apps/zeptoforth',
     '\\ Each case is compiled, so literals are tried as a definition has them, and checks itself. include float64_test.fs, and the last line says how many failed.',
     '', 'float64 import', '', 'variable tests', 'variable fails', '0 tests !', '0 fails !', '40 buffer: text', '',
     ': fail ( -- ) 1 fails +! ." FAIL " tests @ . ;', ': .bits ( lo hi -- ) base @ >r hex u. u. r> base ! ;', '',
     '\\ A value against the bits it should have, to within tol units in the last place',
     ': check { D: got D: want tol -- }', '  1 tests +!', '  got want d- dup 0< if dnegate then 0= swap tol u<= and not if',
     '    fail ." got " got .bits ." want " want .bits cr', '  then', ';', '',
     '\\ A cell against what it should be', ': check-cell { got want -- }', '  1 tests +!', '  got want <> if fail ." got " got . ." want " want . cr then', ';', '',
     '\\ A value as text', ': check-text { D: f addr bytes -- }', '  1 tests +!', '  f text format-dv { count }',
     '  text count addr bytes equal-strings? not if fail ." got " text count type ."  want " addr bytes type cr then', ';', '',
     ': k1 1.25de0 ;', ': k2 k1 k1 dv* ;', '2.5de0 2constant twofive', '2variable dvar', '6.5de0 dvar 2!', '']
n = 0


def case(body):
    global n
    f.append(': test%d %s ; test%d' % (n, body, n))
    n += 1


for fx, want, exact in CASES:
    case('%s %s %d check' % (fx, bits(want), 0 if exact else 1))
for fx, want in LITERALS:
    case('%s %s 0 check' % (fx, bits(want)))
for fx, lo, hi in FIXED:
    case('%s $%08X $%08X 0 check' % (fx, lo, hi))
for fx, want in FLAGS:
    case('%s %d check-cell' % (fx, want))
case('[ hex ] 1de0 [ decimal ] %d check-cell' % 0x1DE0)     # (in hex it is an integer, as 1e0 is with float32)
for fx, want in TEXT:
    case('%s s" %s" check-text' % (fx, want))
f += ['', ': report ( -- ) ." float64: " tests @ . ." tests, " fails @ . ." failed" cr ;', 'report', '']
open(os.path.join(HERE, 'float64_test.fs'), 'w', newline='\n').write(licensed('\n'.join(f)))
print(n, 'cases')
