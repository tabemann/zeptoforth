@ Copyright (c) 2020-2023 Travis Bemann
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
_base:	push {lr}
	bl _cpu_offset
	ldr r0, =dict_base
	adds tos, r0
	ldr tos, [tos]
	adds tos, #base_offset
	pop {pc}
	end_inlined

	@@ Get the PAUSE enabled variable address
	define_word "pause-enabled", visible_flag
_pause_enabled:
	push {lr}
	bl _cpu_offset
	ldr r0, =pause_enabled
	adds tos, r0
	pop {pc}
	end_inlined

	@@ Get the XON/XOFF enabled variable address
	define_word "xon-xoff-enabled", visible_flag
_xon_xoff_enabled:
	push_tos
	ldr tos, =xon_xoff_enabled
	bx lr
	end_inlined

	@@ Get the ACK/NAK enabled variable address
	define_word "ack-nak-enabled", visible_flag
_ack_nak_enabled:
	push_tos
	ldr tos, =ack_nak_enabled
	bx lr
	end_inlined

	@@ Get the BEL enabled variable address
	define_word "bel-enabled", visible_flag
_bel_enabled:
	push_tos
	ldr tos, =bel_enabled
	bx lr
	end_inlined

	@@ Get the RAM dictionary base variable address
	define_word "dict-base", visible_flag
_dict_base:
	push {lr}
	bl _cpu_offset
	ldr r0, =dict_base
	adds tos, r0
	pop {pc}
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
	push {lr}
	bl _cpu_offset
	ldr r0, =dict_base
	adds tos, r0
	ldr tos, [tos]
	adds tos, #stack_base_offset
	pop {pc}
	end_inlined

	@@ Get the current stack end variable address
	define_word "stack-end", visible_flag
_stack_end:
	push {lr}
	bl _cpu_offset
	ldr r0, =dict_base
	adds tos, r0
	ldr tos, [tos]
	adds tos, #stack_end_offset
	pop {pc}
	end_inlined

	@@ Get the current return stack base variable address
	define_word "rstack-base", visible_flag
_rstack_base:
	push {lr}
	bl _cpu_offset
	ldr r0, =dict_base
	adds tos, r0
	ldr tos, [tos]
	adds tos, #rstack_base_offset
	pop {pc}
	end_inlined

	@@ Get the current returns stack end variable address
	define_word "rstack-end", visible_flag
_rstack_end:
	push {lr}
	bl _cpu_offset
	ldr r0, =dict_base
	adds tos, r0
	ldr tos, [tos]
	adds tos, #rstack_end_offset
	pop {pc}
	end_inlined

	@@ Get the current exception handler variable address
	define_word "handler", visible_flag
_handler:
	push {lr}
	bl _cpu_offset
	ldr r0, =dict_base
	adds tos, r0
	ldr tos, [tos]
	adds tos, #handler_offset
	pop {pc}
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

.ltorg
        
        @@ The parse hook
        define_word "parse-hook", visible_flag
_parse_hook:
        push_tos
        ldr tos, =parse_hook
        bx lr
        end_inlined
        
	@@ The find hook
	define_word "find-hook", visible_flag
_find_hook:
	push_tos
	ldr tos, =find_hook
	bx lr
	end_inlined

        @@ The find raw hook
        define_word "find-raw-hook", visible_flag
_find_raw_hook:
        push_tos
        ldr tos, =find_raw_hook
        bx lr
        end_inlined

        @@ The word (including quotation) beginning hook
        define_word "word-begin-hook", visible_flag
_word_begin_hook:
        push_tos
        ldr tos, =word_begin_hook
        bx lr
        end_inlined

        @@ The word exit hook
        define_word "word-exit-hook", visible_flag
_word_exit_hook:
        push_tos
        ldr tos, =word_exit_hook
        bx lr
        end_inlined

        @@ The word end hook
        define_word "word-end-hook", visible_flag
_word_end_hook:
        push_tos
        ldr tos, =word_end_hook
        bx lr
        end_inlined

        @@ The word reset hook
        define_word "word-reset-hook", visible_flag
_word_reset_hook:
        push_tos
        ldr tos, =word_reset_hook
        bx lr
        end_inlined

        @@ The block beginning hook
        define_word "block-begin-hook", visible_flag
_block_begin_hook:
        push_tos
        ldr tos, =block_begin_hook
        bx lr
        end_inlined
        
        @@ The block exit hook
        define_word "block-exit-hook", visible_flag
_block_exit_hook:
        push_tos
        ldr tos, =block_exit_hook
        bx lr
        end_inlined

        @@ The block end hook
        define_word "block-end-hook", visible_flag
_block_end_hook:
        push_tos
        ldr tos, =block_end_hook
        bx lr
        end_inlined

	@@ The finalize hook
	define_word "finalize-hook", visible_flag
_finalize_hook:
	push_tos
	ldr tos, =finalize_hook
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

	@@ The flash mini-dictionary base address
	define_internal_word "flash-mini-dict", visible_flag
_flash_mini_dict:
	push_tos
	ldr tos, =flash_mini_dict
	bx lr
	end_inlined

	@@ The flash mini-dictionary size
	define_internal_word "flash-mini-dict-size", visible_flag
_flash_mini_dict_size:
	push_tos
	ldr tos, =flash_mini_dict_size
	bx lr
	end_inlined

	@@ Whether Thumb-2 is supported
	define_word "thumb-2?", visible_flag
_thumb_2:
	push_tos
	movs tos, #0
	.if thumb2
        mvns tos, tos
	.endif
	bx lr
	end_inlined

        @@ Whether the MCU is a Cortex-M7 MCU
        define_word "cortex-m7?", visible_flag
_cortex_m7:
        push_tos
        movs tos, #0
        .if cortex_m7
        mvns tos, tos
        .endif
        bx lr
        end_inlined

        @@ Get whether the CPU is an RP2040
        define_word "rp2040?", visible_flag
_rp2040:
        push_tos
        movs tos, #0
        .ifdef rp2040
        mvns tos, tos
        .endif
        bx lr
        end_inlined

	@@ Get the CPU count
	define_word "cpu-count", visible_flag | inlined_flag
_cpu_count:
	push_tos
	movs tos, #cpu_count
	bx lr
	end_inlined

        @@ Get the evaluation buffer index pointer address
        define_internal_word "eval-index-ptr", visible_flag
_eval_index_ptr:
        push_tos
        ldr tos, =eval_index_ptr
        bx lr
        end_inlined

        @@ Get the evaluation buffer count pointer address
        define_internal_word "eval-count-ptr", visible_flag
_eval_count_ptr:
        push_tos
        ldr tos, =eval_count_ptr
        bx lr
        end_inlined

        @@ Get the evaluatiaon buffer pointer address
        define_internal_word "eval-ptr", visible_flag
_eval_ptr:
        push_tos
        ldr tos, =eval_ptr
        bx lr
        end_inlined

        @@ Get the evaluation data value address
        define_internal_word "eval-data", visible_flag
_eval_data:
        push_tos
        ldr tos, =eval_data
        bx lr
        end_inlined

        @@ Get the evaluation refill word address
        define_internal_word "eval-refill", visible_flag
_eval_refill:
        push_tos
        ldr tos, =eval_refill
        bx lr
        end_inlined

        @@ Get the evaluation EOF word address
        define_internal_word "eval-eof", visible_flag
_eval_eof:
        push_tos
        ldr tos, =eval_eof
        bx lr
        end_inlined

        @@ Get the prompt disabled value address
        define_internal_word "prompt-disabled", visible_flag
_prompt_disabled:
        push_tos
        ldr tos, =prompt_disabled
        bx lr
        
	.ltorg
	
