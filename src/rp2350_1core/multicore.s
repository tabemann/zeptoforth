@ Copyright (c) 2021-2025 Travis Bemann
@
@ Permission is hereby granted, free of charge, to any person obtaining a copy
@ of this software and associated documentation files (the "Software"), to deal
@ in the Software without restriction, including without limitation the rights
@ to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
@ copies of the Software, and to permit persons to whom the Software is
@ furnished to do so, subject to the following conditions:
@ 
@ The above copyright notice and this permission notice shall be included in
@ all copies or substantial portions of the Software.
@ 
@ THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
@ IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
@ FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
@ AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
@ LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
@ OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
@ SOFTWARE.

	@@ Handle SIO interrupt
_handle_sio:
        bx lr
        end_inlined

	@@ Loop forever handling the FIFO
	define_internal_word "loop-forever-fifo", visible_flag
_loop_forever_fifo:
        bx lr
	end_inlined

	@@ Force the other core to wait
	define_internal_word "force-core-wait", visible_flag
_force_core_wait:
        bx lr
	end_inlined

	@@ Release the other core
	define_internal_word "release-core", visible_flag
_release_core:
        bx lr
	end_inlined

	@@ Wait if necessary if a wait has already begun
	define_internal_word "wait-current-core", visible_flag
_wait_current_core:
        bx lr
	end_inlined

	@@ Hold a core in a loop
	define_internal_word "hold-core", visible_flag
_hold_core:
        bx lr
	end_inlined

	.ltorg
