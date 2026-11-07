"""make_armv7m_fp64_test.py [AS]: writes armv7m_fp64_test.fs, which checks src/common/forth/armv7m_fp64.fs against GNU as. Each case is assembled here by as, and the test file holds the halfwords it made, so the
expected values do not come from the code they check. AS is arm-none-eabi-as, if it is not on the path. The test only assembles, so it runs on any target, with or without a double-precision FPU.

With --f32=PATH, PATH being armv7m_fp.fs, it also takes each instruction there and prints those that as encodes differently: a report, not part of the test."""
import os, re, struct, subprocess, sys, tempfile
HERE = os.path.dirname(os.path.abspath(__file__))
argv = [a for a in sys.argv[1:] if not a.startswith('--')]
AS = argv[0] if argv else 'arm-none-eabi-as'
OBJCOPY = re.sub(r'as(\.exe)?$', r'objcopy\1', AS)

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


R3 = [(5, 3, 1), (0, 15, 7), (15, 0, 8)]            # vm, vn, vd
R2 = [(5, 1), (15, 0), (0, 14)]                     # vm, vd
DS = [(5, 3), (15, 30), (0, 1), (8, 16)]            # a double, a single
FB16 = [0, 1, 8, 16]
FB32 = [1, 8, 32]


def three(n, g):
    return [('d%d d%d d%d %s' % (m, a, d, n), '%s d%d, d%d, d%d' % (g, d, a, m)) for m, a, d in R3]


def two(n, g):
    return [('d%d d%d %s' % (m, d, n), '%s d%d, d%d' % (g, d, m)) for m, d in R2]


def zero(n, g):
    return [('d%d %s' % (d, n), '%s d%d, #0.0' % (g, d)) for d in (0, 7, 15)]


def d2s(n, g):
    return [('d%d s%d %s' % (d, s, n), '%s s%d, d%d' % (g, s, d)) for d, s in DS]


def s2d(n, g):
    return [('s%d d%d %s' % (s, d, n), '%s d%d, s%d' % (g, d, s)) for d, s in DS]


def fract(n, g, bits):
    return [('%d d%d %s' % (f, d, n), '%s d%d, d%d, #%d' % (g, d, d, f)) for f in bits for d in (3, 15)]


def ldst(n, g):
    return [('%d r%d d%d %s' % (i, r, d, n), '%s d%d, [r%d, #%d]' % (g, d, r, i)) for i, r, d in ((0, 2, 3), (8, 2, 3), (-8, 7, 15), (1020, 0, 0), (-1020, 5, 1))]


def regs(s, e):
    return '{d%d}' % s if s == e else '{d%d-d%d}' % (s, e)


def multi(n, g, bang):
    return [('d%d d%d r%d %s' % (e, s, r, n), '%s r%d%s, %s' % (g, r, bang, regs(s, e))) for s, e, r in ((3, 5, 2), (0, 15, 7), (15, 15, 0), (8, 9, 4))]


CASES = []
for n in 'vadd vdiv vfma vfms vfnma vfnms vmaxnm vminnm vmla vmls vmul vnmla vnmls vnmul vseleq vselvs vselge vselgt vsub'.split():
    CASES += three('%s.f64_,_,_' % n, n + '.f64')
for n in 'vabs vcmp vcmpe vmov vneg vrinta vrintn vrintp vrintm vrintx vrintz vrintr vsqrt'.split():
    CASES += two('%s.f64_,_' % n, n + '.f64')
for n in 'vcmp vcmpe'.split():
    CASES += zero('%s.f64_,#0.0' % n, n + '.f64')
for n in 'vcvta vcvtn vcvtp vcvtm vcvt vcvtr'.split():
    for t in 'u32 s32'.split():
        CASES += d2s('%s.%s.f64_,_' % (n, t), '%s.%s.f64' % (n, t))
for n, g in (('vcvt.f32.f64_,_', 'vcvt.f32.f64'), ('vcvtb.f16.f64_,_', 'vcvtb.f16.f64'), ('vcvtt.f16.f64_,_', 'vcvtt.f16.f64')):
    CASES += d2s(n, g)
for n, g in (('vcvt.f64.u32_,_', 'vcvt.f64.u32'), ('vcvt.f64.s32_,_', 'vcvt.f64.s32'), ('vcvt.f64.f32_,_', 'vcvt.f64.f32'), ('vcvtb.f64.f16_,_', 'vcvtb.f64.f16'), ('vcvtt.f64.f16_,_', 'vcvtt.f64.f16')):
    CASES += s2d(n, g)
for t, bits in (('u16', FB16), ('s16', FB16), ('u32', FB32), ('s32', FB32)):
    CASES += fract('vcvt.%s.f64_,#<fbits>' % t, 'vcvt.%s.f64' % t, bits)
    CASES += fract('vcvt.f64.%s_,#<fbits>' % t, 'vcvt.f64.%s' % t, bits)
CASES += ldst('vldr.f64_,[_,#_]', 'vldr') + ldst('vstr.f64_,[_,#_]', 'vstr')
CASES += multi('vldmia.f64_,{_..._}', 'vldmia', '') + multi('vldmia!.f64_,{_..._}', 'vldmia', '!') + multi('vldmdb!.f64_,{_..._}', 'vldmdb', '!')
CASES += multi('vstmia.f64_,{_..._}', 'vstmia', '') + multi('vstmia!.f64_,{_..._}', 'vstmia', '!') + multi('vstmdb!.f64_,{_..._}', 'vstmdb', '!')
CASES += [('%d %d d%d vmov.f64_,#<imm4h>,#<imm4l>' % (i & 15, i >> 4, d), 'vmov.f64 d%d, #%s' % (d, v)) for i, d, v in ((0x70, 3, '1.0'), (0x00, 0, '2.0'), (0x80, 15, '-2.0'), (0x60, 5, '0.5'), (0x24, 7, '10.0'))]
CASES += [('d%d r%d r%d vmov.cr.f64_,_,_' % (d, b, a), 'vmov r%d, r%d, d%d' % (a, b, d)) for d, a, b in ((3, 1, 2), (15, 0, 7), (0, 6, 5))]
CASES += [('r%d r%d d%d vmov.f64.cr_,_,_' % (b, a, d), 'vmov d%d, r%d, r%d' % (d, a, b)) for d, a, b in ((3, 1, 2), (15, 0, 7), (0, 6, 5))]
CASES += [('d%d d%d vpop.f64{_..._}' % (e, s), 'vpop ' + regs(s, e)) for s, e in ((3, 5), (0, 15), (8, 8))]
CASES += [('d%d d%d vpush.f64{_..._}' % (e, s), 'vpush ' + regs(s, e)) for s, e in ((3, 5), (0, 15), (8, 8))]


def assemble(lines):
    """each line alone, as a 32-bit instruction: the halfwords as makes"""
    with tempfile.TemporaryDirectory() as t:
        s, o, b = os.path.join(t, 'a.s'), os.path.join(t, 'a.o'), os.path.join(t, 'a.bin')
        open(s, 'w').write('\t.syntax unified\n\t.thumb\n\t.text\n' + ''.join('\t%s\n' % l for l in lines))
        subprocess.run([AS, '-mcpu=cortex-m7', '-mfpu=fpv5-d16', '-mthumb', s, '-o', o], check=True)
        subprocess.run([OBJCOPY, '-O', 'binary', '-j', '.text', o, b], check=True)
        d = open(b, 'rb').read()
    assert len(d) == 4 * len(lines), (len(d), len(lines))
    return [struct.unpack_from('<HH', d, i * 4) for i in range(len(lines))]


def f32_report():
    """armv7m_fp.fs's own table against as, in s1, s2, s3"""
    src = open(sys.argv[[a.startswith('--f32') for a in sys.argv].index(True)].split('=', 1)[1]).read()
    bad = 0
    for h0, h1, kind, name in re.findall(r'\$([0-9A-F]{4}) \$([0-9A-F]{4})(?: \d+)?(?: true| false)? (instr-\S+) (\S+)', src):
        m = re.match(r'(v[a-z0-9]+!?)((?:\.[a-z0-9]+)*)_', name)
        if m is None:
            continue
        g = m.group(1) + m.group(2)
        line = {'instr-2*sr': g + ' s0, s0', 'instr-3*sr': g + ' s0, s0, s0', 'instr-sr': g + ' s0, #0.0'}.get(kind)
        if g.startswith('vcvt') and re.match(r'vcvt[anpm]?r?\.[us]32\.f32|vcvt\.f32\.[us]32', g):
            line = g + ' s0, s0'
        if kind == 'instr-sr-fract':
            size = 16 if '16' in g else 32
            line = g + ' s0, s0, #%d' % size            # the most fraction bits: imm 0, as its words would have to be given
        if 'vmov.cr.f32_,_,_' in name:
            line = 'vmov r0, r1, s0, s1'
        if 'vmov.f32.cr_,_,_' in name:
            line = 'vmov s0, s1, r0, r1'
        if line is None:
            continue
        try:
            got = assemble([line])[0]
        except subprocess.CalledProcessError:
            print('as refuses', line)
            continue
        want = (int(h0, 16), int(h1, 16))
        mask = (0xFFFF, 0xFFFF)
        if 'vmov r0, r1' in line or 'vmov s0, s1' in line:
            mask = (0xFFF0, 0x0FF0)
        if (got[0] & mask[0], got[1] & mask[1]) != (want[0] & mask[0], want[1] & mask[1]):
            print('%-28s as %04X %04X, armv7m_fp.fs %04X %04X  (%s)' % (name, got[0], got[1], want[0], want[1], line))
            bad += 1
    print(bad, 'differ')


if any(a.startswith('--f32') for a in sys.argv):
    f32_report()
    sys.exit(0)

words = assemble([g for _, g in CASES])
f = ['\\ armv7m_fp64_test.fs: the double-precision instructions of src/common/forth/armv7m_fp64.fs against GNU as. Written by make_armv7m_fp64_test.py, which has the cases: change them there.',
     '\\ Load armv7m_fp.fs and armv7m_fp64.fs first. This only assembles, so it runs on any target. It ends by saying how many cases failed.', '',
     'variable start', 'variable tests', 'variable fails', '0 tests !', '0 fails !', '',
     ': begin-case ( -- ) here start ! ;', '',
     '\\ What was assembled against what as made, and the space given back',
     ': check { h0 h1 -- }', '  1 tests +!', '  start @ h@ h0 <> start @ 2 + h@ h1 <> or here start @ - 4 <> or if',
     '    1 fails +!', '    ." FAIL case " tests @ . ." got " base @ >r hex start @ h@ u. start @ 2 + h@ u. ." want " h0 u. h1 u. r> base ! cr', '  then',
     '  start @ here - allot', ';', '',
     ': report ( -- ) ." armv7m_fp64: " tests @ . ." cases, " fails @ . ." failed" cr ;', '',
     'armv7m-fp import', 'armv6m::armv6m-instr import', 'armv7m-fp::armv7m-fp-instr import', '']
for (fx, g), (a, b) in zip(CASES, words):
    f.append('begin-case %s $%04X $%04X check \\ %s' % (fx, a, b, g))
f += ['', 'report', '']
open(os.path.join(HERE, 'armv7m_fp64_test.fs'), 'w', newline='\n').write(licensed('\n'.join(f)))
print(len(CASES), 'cases,', len({c[0].split()[-1] for c in CASES}), 'instructions')
