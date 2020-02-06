@ Copyright (c) 2020 Travis Bemann
@
@ This program is free software: you can redistribute it and/or modify
@ it under the terms of the GNU General Public License as published by
@ the Free Software Foundation, either version 3 of the License, or
@ (at your option) any later version.
@
@ This program is distributed in the hope that it will be useful,
@ but WITHOUT ANY WARRANTY; without even the implied warranty of
@ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
@ GNU General Public License for more details.
@
@ You should have received a copy of the GNU General Public License
@ along with this program.  If not, see <http://www.gnu.org/licenses/>.

	@@ Get the STATE variable address
	define_word "state", visible_flag
_state:	push_tos
	ldr tos, =state
	bx lr

	@@ Expose the building flag
	define_word "building", visible_flag
_building:
	push_tos
	ldr tos, =building
	bx lr

	@@ Get the BASE variable address
	define_word "base", visible_flag
_base:	push_tos
	ldr tos, =base
	bx lr

	@@ The input buffer index
	define_word ">in", visible_flag
_to_in:	push_tos
	ldr tos, =input_buffer_index
	bx lr

	@@ The input buffer count
	define_word "input#", visible_flag
_input_count:
	push_tos
	ldr tos, =input_buffer_count
	bx lr	

	@@ The input buffer
	define_word "input", visible_flag
_input:	push_tos
	ldr tos, =input_buffer
	bx lr

	@@ The prompt hook
	define_word "prompt-hook", visible_flag
_prompt_hook:
	push_tos
	ldr tos, =prompt_hook
	bx lr

	@@ The handle number hook
	define_word "handle-number-hook", visible_flag
_handle_number_hook:
	push_tos
	ldr tos, =handle_number_hook
	bx lr

	@@ The failed parse hook
	define_word "failed-parse-hook", visible_flag
_failed_parse_hook:
	push_tos
	ldr tos, =failed_parse_hook
	bx lr

	@@ The emit hook
	define_word "emit-hook", visible_flag
_emit_hook:
	push_tos
	ldr tos, =emit_hook
	bx lr

	@@ The emit? hook
	define_word "emit?-hook", visible_flag
_emit_q_hook:
	push_tos
	ldr tos, =emit_q_hook
	bx lr

	@@ The key hook
	define_word "key-hook", visible_flag
_key_hook:
	push_tos
	ldr tos, =key_hook
	bx lr

	@@ The key? hook
	define_word "key?-hook", visible_flag
_key_q_hook:
	push_tos
	ldr tos, =key_q_hook
	bx lr

	@@ The refill hook
	define_word "refill-hook", visible_flag
_refill_hook:
	push_tos
	ldr tos, =refill_hook
	bx lr

	@@ The pause hook
	define_word "pause-hook", visible_flag
_pause_hook:
	push_tos
	ldr tos, =pause_hook
	bx lr

	@@ Get the FAULT-HANDLER-HOOK variable address
	define_word "fault-handler-hook", visible_flag
_fault_handler_hook:
	push_tos
	ldr tos, =fault_handler_hook
	bx lr

	@@ Get the NULL-HANDLER-HOOK variable address
	define_word "null-handler-hook", visible_flag
_null_handler_hook:
	push_tos
	ldr tos, =null_handler_hook
	bx lr

	@@ Get the SYSTICK-HANDLER-HOOK variable address
	define_word "systick-handler-hook", visible_flag
_systick_handler_hook:
	push_tos
	ldr tos, =systick_handler_hook
	bx lr
