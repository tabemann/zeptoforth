@ Copyright (c) 2019-2022 Travis Bemann
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

	.ifndef rp2040

	@@ Vector table in RAM
	allot vector_table, vector_table_size

	.else

	.equ vector_table, ram_real_start

	.endif

	@@ The stack base offset
	.equ stack_base_offset, 0

	@@ The stack end offset
	.equ stack_end_offset, 4

	@@ The return stack base offset
	.equ rstack_base_offset, 8

	@@ The return stack end offset
	.equ rstack_end_offset, 12

	@@ The RAM HERE offset
	.equ ram_here_offset, 16

	@@ The base offset
	.equ base_offset, 20

	@@ The handler offset
	.equ handler_offset, 24

	@@ The initial USER offset
	.equ user_offset, 28

	@@ Pointer to the current Flash HERE location
	allot flash_here, 4

	@@ Pointer to the base of HERE space
	allot dict_base, 4 * cpu_count

	@@ Flag to determine whether compilation is going to Flash
	allot compiling_to_flash, 4

	@@ Flash buffers
	allot flash_buffers_start, flash_buffer_size * flash_buffer_count	

	@@ The word being currently compiled
	allot current_compile, 4

	@@ The current deferred literal
	allot deferred_literal, 4

	@@ Whether there is a deferred literal
	allot literal_deferred_q, 4
	
	@@ The last word compiled
	allot latest, 4

	@@ The last word compiled to RAM
	allot ram_latest, 4

	@@ The last word compiled to flash
	allot flash_latest, 4

	@@ The compilation wordlist
	allot wordlist, 4

	@@ The current <BUILDS target address
	allot build_target, 4

	@@ The flags for the word being currently compiled
	allot current_flags, 4

	@@ Suppress inlining
	allot suppress_inline, 4

	@@ The evaluation buffer index pointer
	allot eval_index_ptr, 4

	@@ The evaluation buffer count pointer
	allot eval_count_ptr, 4

	@@ The evaluation buffer pointer
	allot eval_ptr, 4

	@@ The current input buffer index
	allot input_buffer_index, 4

	@@ The input buffer count
	allot input_buffer_count, 4

	@@ The input buffer
	allot input_buffer, input_buffer_size + 1

	@@ Are we in compilation state
	allot state, 4

	@@ Is PAUSE enabled (enabled > 0)
	allot pause_enabled, 4 * cpu_count

	@@ Is compress flash enabled
	allot compress_flash_enabled, 4
	
	@@ The prompt hook
	allot prompt_hook, 4

	@@ The number parser hook
	allot handle_number_hook, 4

	@@ The failed parse hook
	allot failed_parse_hook, 4

	@@ The emit hook
	allot emit_hook, 4

	@@ The emit? hook
	allot emit_q_hook, 4

	@@ The key hook
	allot key_hook, 4

	@@ The key? hook
	allot key_q_hook, 4

	@@ The refill hook
	allot refill_hook, 4

	@@ The pause hook
	allot pause_hook, 4

	@@ The dictionary size validation hook
	allot validate_dict_hook, 4

	@@ The wordlist count
	allot order_count, 4

	@@ The find hook
	allot find_hook, 4

	@@ The finalize hook
	allot finalize_hook, 4
	
	@@ The wordlist order
	allot order, 2 * max_order_size
	
	@@ The flash mini-dictionary
	allot flash_mini_dict, flash_mini_dict_size
	
