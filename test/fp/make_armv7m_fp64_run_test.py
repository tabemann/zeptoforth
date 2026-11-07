"""make_armv7m_fp64_run_test.py: writes armv7m_fp64_run_test.fs, which runs the instructions of src/common/forth/armv7m_fp64.fs on a double-precision FPU and checks the results by their bits. The expected values
are worked out here with Python's IEEE doubles. Unlike armv7m_fp64_test.fs it needs a Cortex-M7 with FPv5-D16: on another target the first instruction faults."""
import math, os, struct, textwrap
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


def bits(x):
    b = struct.unpack('<Q', struct.pack('<d', x))[0]
    return b & 0xFFFFFFFF, b >> 32


def lit(x):
    return '$%08X $%08X' % bits(x)


def f32(x):
    return struct.unpack('<I', struct.pack('<f', x))[0]


POP2 = 'r0 1 dp ldm tos r0 %s vmov.f64.cr_,_,_ tos 1 dp ldm'        # ( lo hi -- ) into a register: the stack's next cell in tos, as pull_tos leaves it
PUSH = '%s tos r0 vmov.cr.f64_,_,_ 4 dp subs_,#_ 0 dp r0 str_,[_,#_]'  # a register onto the stack, after the cell below has been saved
SAVE = '4 dp subs_,#_ 0 dp tos str_,[_,#_]'


def binary(op, a, b, want):
    return (lit(a) + ' ' + lit(b), '%s %s %s d1 d0 d2 %s %s %s' % (POP2 % 'd1', POP2 % 'd0', '', op, SAVE, PUSH % 'd2'), 2, want)


def unary(op, a, want):
    return (lit(a), '%s d0 d1 %s %s %s' % (POP2 % 'd0', op, SAVE, PUSH % 'd1'), 2, want)


def fused(op, c, a, b, want):
    return (lit(c) + ' ' + lit(a) + ' ' + lit(b), '%s %s %s d1 d0 d2 %s %s %s' % (POP2 % 'd1', POP2 % 'd0', POP2 % 'd2', op, SAVE, PUSH % 'd2'), 2, want)


T = []
for op, a, b, w in (('vadd.f64_,_,_', 1.5, 2.25, 3.75), ('vsub.f64_,_,_', 1.0, 3.0, -2.0), ('vmul.f64_,_,_', 1.5, 2.25, 3.375), ('vdiv.f64_,_,_', 1.0, 3.0, 1 / 3),
                    ('vnmul.f64_,_,_', 1.5, 2.0, -3.0), ('vmaxnm.f64_,_,_', 1.0, 5.0, 5.0), ('vminnm.f64_,_,_', 1.0, 5.0, 1.0)):
    T.append(binary(op, a, b, bits(w)))
for op, a, w in (('vsqrt.f64_,_', 2.0, math.sqrt(2)), ('vneg.f64_,_', 7.0, -7.0), ('vabs.f64_,_', -7.0, 7.0), ('vmov.f64_,_', 2.5, 2.5), ('vrinta.f64_,_', 2.5, 3.0), ('vrintn.f64_,_', 2.5, 2.0),
                 ('vrintp.f64_,_', 2.1, 3.0), ('vrintm.f64_,_', -2.1, -3.0), ('vrintz.f64_,_', -2.9, -2.0), ('vrintr.f64_,_', 3.5, 4.0)):
    T.append(unary(op, a, bits(w)))
c, a, b = 7.0, 3.0, 5.0
for op, w in (('vmla.f64_,_,_', c + a * b), ('vmls.f64_,_,_', c - a * b), ('vnmla.f64_,_,_', -c - a * b), ('vnmls.f64_,_,_', -c + a * b),
              ('vfma.f64_,_,_', c + a * b), ('vfms.f64_,_,_', c - a * b), ('vfnma.f64_,_,_', -c - a * b), ('vfnms.f64_,_,_', -c + a * b)):
    T.append(fused(op, c, a, b, bits(w)))

# conversions: to and from a core register through s0
TO_INT = '%s d0 s0 %%s s0 tos vmov.cr.f32_,_' % (POP2 % 'd0')
for op, a, w in (('vcvt.s32.f64_,_', -7.5, -7), ('vcvtr.s32.f64_,_', 2.5, 2), ('vcvta.s32.f64_,_', 2.5, 3), ('vcvtn.s32.f64_,_', 3.5, 4), ('vcvtp.s32.f64_,_', 2.1, 3), ('vcvtm.s32.f64_,_', -2.1, -3),
                 ('vcvt.u32.f64_,_', 3000000000.0, 3000000000), ('vcvta.u32.f64_,_', 2.5, 3)):
    T.append((lit(a), (TO_INT % op).replace(' tos 1 dp ldm d0', ' d0') + ' ', 1, w & 0xFFFFFFFF))
FROM_INT = 'tos s0 vmov.f32.cr_,_ s0 d0 %s ' + PUSH % 'd0'
T.append(('-7', FROM_INT % 'vcvt.f64.s32_,_', 'int', bits(-7.0)))
T.append(('$FFFFFFFF', FROM_INT % 'vcvt.f64.u32_,_', 'int', bits(4294967295.0)))
T.append(('$%08X' % f32(0.5), FROM_INT % 'vcvt.f64.f32_,_', 'int', bits(0.5)))
T.append((lit(0.5), ((POP2 % 'd0').replace(' tos 1 dp ldm', '')) + ' d0 s0 vcvt.f32.f64_,_ s0 tos vmov.cr.f32_,_', 1, f32(0.5)))
T.append((lit(1.5), ((POP2 % 'd0').replace(' tos 1 dp ldm', '')) + ' 8 d0 vcvt.s32.f64_,#<fbits> d0 tos r0 vmov.cr.f64_,_,_ r0 tos movs_,_', 1, 384))
T.append((lit(1.5), ((POP2 % 'd0').replace(' tos 1 dp ldm', '')) + ' 8 d0 vcvt.s32.f64_,#<fbits> 8 d0 vcvt.f64.s32_,#<fbits> ' + PUSH.replace('4 dp subs_,#_ ', '4 dp subs_,#_ ') % 'd0', 'same', bits(1.5)))
# immediate, memory, selection
T.append(('0', 'tos 1 dp ldm 0 7 d0 vmov.f64_,#<imm4h>,#<imm4l> ' + SAVE + ' ' + PUSH % 'd0', 'imm', bits(1.0)))
T.append((lit(2.75), (POP2 % 'd0') + ' d0 d0 vpush.f64{_..._} d1 d1 vpop.f64{_..._} ' + SAVE + ' ' + PUSH % 'd1', 2, bits(2.75)))
T.append((lit(2.75), (POP2 % 'd0') + ' 8 dp subs_,#_ 0 dp d0 vstr.f64_,[_,#_] 0 dp d1 vldr.f64_,[_,#_] 8 dp adds_,#_ ' + SAVE + ' ' + PUSH % 'd1', 2, bits(2.75)))
for op, a, b, w in (('vselgt.f64_,_,_', 2.0, 1.0, 2.0), ('vselgt.f64_,_,_', 1.0, 2.0, 2.0), ('vseleq.f64_,_,_', 3.0, 3.0, 3.0), ('vselge.f64_,_,_', 1.0, 2.0, 2.0)):
    T.append((lit(a) + ' ' + lit(b), '%s %s d1 d0 vcmp.f64_,_ apsr-nzcv vmrs_,fpscr d1 d0 d2 %s %s %s' % (POP2 % 'd1', POP2 % 'd0', op, SAVE, PUSH % 'd2'), 2, bits(w)))

f = ['\\ armv7m_fp64_run_test.fs: the instructions of src/common/forth/armv7m_fp64.fs run on a double-precision FPU, their results checked by their bits. Written by make_armv7m_fp64_run_test.py.',
     '\\ Load armv7m_fp.fs and armv7m_fp64.fs first. It needs a Cortex-M7 with FPv5-D16, and it ends by saying how many cases failed.', '',
     'armv7m-fp import', '',
     'variable tests', 'variable fails', '0 tests !', '0 fails !', '',
     ': bad ( -- ) 1 fails +! ." FAIL case " tests @ . cr ;',
     ': check2 { D: got D: want -- } 1 tests +! got want d= not if bad ." got " got swap base @ >r hex u. u. r> base ! cr then ;',
     ': check1 { got want -- } 1 tests +! got want <> if bad ." got " got . cr then ;', '']
for i, (args, body, kind, want) in enumerate(T):
    f += textwrap.wrap(': t%d code[ %s ]code ;' % (i, ' '.join(body.split())), 100, break_long_words=False, break_on_hyphens=False, subsequent_indent='  ')  # the console's lines hold 255 characters
    if isinstance(want, tuple):
        f.append('%s t%d $%08X $%08X check2' % (args, i, want[0], want[1]))
    else:
        f.append('%s t%d $%08X check1' % (args, i, want))
f += ['', ': report ( -- ) ." armv7m_fp64 run: " tests @ . ." cases, " fails @ . ." failed" cr ;', 'report', '']
open(os.path.join(HERE, 'armv7m_fp64_run_test.fs'), 'w', newline='\n').write(licensed('\n'.join(f)))
print(len(T), 'cases')
