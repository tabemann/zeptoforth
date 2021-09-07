@ Copyright (c) 2020-2021 Travis Bemann
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

	@@ Get the STATE variable address
	define_word "state", visible_flag
_state:	push_tos
	ldr tos, =state
	bx lr
	end_inlined

	@@ Get the BASE variable address
	define_word "base", visible_flag
_base:	push_tos
	ldr tos, =base
	bx lr
	end_inlined

	@@ Get the PAUSE enabled variable address
	define_word "pause-enabled", visible_flag
_pause_enabled:
	push_tos
	ldr tos, =pause_enabled
	bx lr
	end_inlined

	@@ Get the RAM dictionary base variable address
	define_word "dict-base", visible_flag
_dict_base:
	push_tos
	ldr tos, =dict_base
	bx lr
	end_inlined
	
	@@ Get the RAM base
	define_word "ram-base", visible_flag
_ram_base:
	push_tos
	ldr tos, =ram_start
	bx lr
	end_inlined

	@@ Get the RAM end
	define_word "ram-end", visible_flag
_ram_end:
	push_tos
	ldr tos, =ram_end
	bx lr
	end_inlined

	@@ Get the flash base
	define_word "flash-base", visible_flag
_flash_base:
	push_tos
	ldr tos, =flash_start
	bx lr
	end_inlined

	@@ Get the flash end
	define_word "flash-end", visible_flag
_flash_end:
	push_tos
	ldr tos, =flash_dict_end
	bx lr
	end_inlined
	
	@@ Get the current stack base variable address
	define_word "stack-base", visible_flag
_stack_base:
	push_tos
	ldr tos, =stack_base
	bx lr
	end_inlined

	@@ Get the current stack end variable address
	define_word "stack-end", visible_flag
_stack_end:
	push_tos
	ldr tos, =stack_end
	bx lr
	end_inlined

	@@ Get the current return stack base variable address
	define_word "rstack-base", visible_flag
_rstack_base:
	push_tos
	ldr tos, =rstack_base
	bx lr
	end_inlined

	@@ Get the current returns stack end variable address
	define_word "rstack-end", visible_flag
_rstack_end:
	push_tos
	ldr tos, =rstack_end
	bx lr
	end_inlined

	@@ Get the current exception handler variable address
	define_word "handler", visible_flag
_handler:
	push_tos
	ldr tos, =handler
	bx lr
	end_inlined

	@@ The parse index
	define_word ">parse", visible_flag
_to_parse:
	push_tos
	ldr tos, =eval_index_ptr
	ldr tos, [tos]
	bx lr
	end_inlined

	@@ THe parse count
	define_word "parse#", visible_flag
_parse_count:	
	push_tos
	ldr tos, =eval_count_ptr
	ldr tos, [tos]
	ldr tos, [tos]
	bx lr
	end_inlined

	@@ The source info
	define_word "parse-buffer", visible_flag
_parse_buffer:
	push_tos
	ldr tos, =eval_ptr
	ldr tos, [tos]
	bx lr
	end_inlined

	.ltorg
	
	@@ The source info
	define_word "source", visible_flag
_source:
	push_tos
	ldr tos, =eval_ptr
	ldr tos, [tos]
	push_tos
	ldr tos, =eval_count_ptr
	ldr tos, [tos]
	ldr tos, [tos]
	bx lr
	end_inlined

	@@ Get the address to store a literal in for the word currently being
	@@ built
	define_word "build-target", visible_flag
_build_target:
	push_tos
	ldr tos, =build_target
	bx lr
	end_inlined

	@@ Get the base of the system RAM dictionary space
	define_word "sys-ram-dict-base", visible_flag
_sys_ram_dict_base:
	push_tos
	ldr tos, =ram_current
	bx lr
	end_inlined

	@@ The input buffer index
	define_word ">in", visible_flag
_to_in:	push_tos
	ldr tos, =input_buffer_index
	bx lr
	end_inlined

	@@ The input buffer count
	define_word "input#", visible_flag
_input_count:
	push_tos
	ldr tos, =input_buffer_count
	bx lr
	end_inlined

	@@ The input buffer
	define_word "input", visible_flag
_input:	push_tos
	ldr tos, =input_buffer
	bx lr
	end_inlined

	@@ The input buffer size
	define_word "input-size", visible_flag
_input_size:
	push_tos
	ldr tos, =input_buffer_size
	bx lr
	end_inlined

	@@ The wordlist count
	define_word "order-count", visible_flag
_order_count:
	push_tos
	ldr tos, =order_count
	bx lr
	end_inlined

	@@ The wordlist order
	define_word "order", visible_flag
_order: push_tos
	ldr tos, =order
	bx lr
	end_inlined
	
	@@ The prompt hook
	define_word "prompt-hook", visible_flag
_prompt_hook:
	push_tos
	ldr tos, =prompt_hook
	bx lr
	end_inlined

	@@ The handle number hook
	define_word "handle-number-hook", visible_flag
_handle_number_hook:
	push_tos
	ldr tos, =handle_number_hook
	bx lr
	end_inlined

	@@ The failed parse hook
	define_word "failed-parse-hook", visible_flag
_failed_parse_hook:
	push_tos
	ldr tos, =failed_parse_hook
	bx lr
	end_inlined

	@@ The emit hook
	define_word "emit-hook", visible_flag
_emit_hook:
	push_tos
	ldr tos, =emit_hook
	bx lr
	end_inlined

	@@ The emit? hook
	define_word "emit?-hook", visible_flag
_emit_q_hook:
	push_tos
	ldr tos, =emit_q_hook
	bx lr
	end_inlined

	@@ The key hook
	define_word "key-hook", visible_flag
_key_hook:
	push_tos
	ldr tos, =key_hook
	bx lr
	end_inlined

	@@ The key? hook
	define_word "key?-hook", visible_flag
_key_q_hook:
	push_tos
	ldr tos, =key_q_hook
	bx lr
	end_inlined

	@@ The refill hook
	define_word "refill-hook", visible_flag
_refill_hook:
	push_tos
	ldr tos, =refill_hook
	bx lr
	end_inlined

	@@ The pause hook
	define_word "pause-hook", visible_flag
_pause_hook:
	push_tos
	ldr tos, =pause_hook
	bx lr
	end_inlined

	@@ The dictionary size validation hook
	define_word "validate-dict-hook", visible_flag
_validate_dict_hook:
	push_tos
	ldr tos, =validate_dict_hook
	bx lr
	end_inlined

	@@ The vector table address
	define_word "vector-table", visible_flag
_vector_table:
	push_tos
	ldr tos, =vector_table
	bx lr
	end_inlined

	@@ The vector count
	define_word "vector-count", visible_flag
_vector_count:
	push_tos
	ldr tos, =vector_count
	bx lr
	end_inlined
	
	.ltorg
	
