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

	@@ Initialize the common handler hooks
	define_word "init-common-handlers", visible_flag
_init_common_handlers:
	movs r0, #0
	ldr r1, =fault_handler_hook
	str r0, [r1]
	ldr r1, =null_handler_hook
	str r0, [r1]
	ldr r1, =systick_handler_hook
	str r0, [r1]
	bx lr
	
	@@ The fault handler
handle_fault:
	ldr r0, =fault_handler_hook
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	adds r0, #1
	bx r0
1:	bx lr

	@@ The null handler
handle_null:
	ldr r0, =null_handler_hook
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	adds r0, #1
	bx r0
1:	bx lr

	@@ The systick handler
handle_systick:
	ldr r0, =systick_handler_hook
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	adds r0, #1
	bx r0
1:	bx lr
