# Copyright (c) 2021 Travis Bemann
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

DATE:=$(shell date)

kernel_info:
	rm -f kernel_info.s
	echo '        define_word "kernel-platform", visible_flag' > kernel_info.s
	echo '_kernel_platform:' >> kernel_info.s
	echo '        push {lr}' >> kernel_info.s
	echo '        string "'$(PLATFORM)'"' >> kernel_info.s
	echo '        pop {pc}' >> kernel_info.s
	echo '        end_inlined' >> kernel_info.s
	echo >> kernel_info.s
	echo '        define_word "kernel-version", visible_flag' >> kernel_info.s
	echo '_kernel_version:' >> kernel_info.s
	echo '        push {lr}' >> kernel_info.s
	echo '        string "'$(VERSION)'"' >> kernel_info.s
	echo '        pop {pc}' >> kernel_info.s
	echo '        end_inlined' >> kernel_info.s
	echo >> kernel_info.s
	echo '        define_word "kernel-date", visible_flag' >> kernel_info.s
	echo '_kernel_date:' >> kernel_info.s
	echo '        push {lr}' >> kernel_info.s
	echo '        string "'$(DATE)'"' >> kernel_info.s
	echo '        pop {pc}' >> kernel_info.s
	echo '        end_inlined' >> kernel_info.s
	echo >> kernel_info.s

.PHONY: kernel_info

clean:
	rm -f kernel_info.s
