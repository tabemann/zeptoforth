@ Copyright (c) 2019-2024 Travis Bemann
@ Copyright (c) 2024 Paul Koning
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
	
	.include "../m33/macro.s"
	.include "../rp2350/variables.s"
	
	.text

	.include "../rp2350/vectors.s"

	@@ The first (null) dictionary entry
	.p2align 2
	.word invisible_flag
	.word 0
10:	.byte 0
	.p2align 2
	
	@@ The entry point
 _handle_reset:
	@@ Initialize r11, relied upon by swdcom
	movs r0, #0
	mov r11, r0
	@@ Initialize the top of stack register
	ldr tos, =0xFEDCBA98
	@@ Get the dictionary base
	ldr r0, =dict_base
	ldr r1, =ram_current
	str r1, [r0]
	@@ Set a garbage dictionary base for the second core
	ldr r0, =dict_base + 4
	ldr r1, =0xEFBEADDE
	str r1, [r0]
	@@ Initialize HERE
	ldr r0, =ram_current + ram_here_offset
	ldr r1, =ram_current + user_offset
	str r1, [r0]
	@@ Initialize the data stack pointer
	ldr r1, =stack_top
	movs dp, r1
	@@ Initialize the return stack pointer
	ldr r1, =rstack_top
	mov sp, r1
	@@ Put a garbage value in HANDLER to force a crash if is used
	ldr r0, =ram_current + handler_offset
	ldr r1, =0xF0E1C2D3
	str r1, [r0]
	@@ Call the rest of the runtime in an exception handler
2:	push_tos
	ldr tos, =outer_exc_handled
	bl _try
	@@ If the inner loop returns, reboot
	b _handle_reset

	@@ The outermost exception handling - if an exception happens here the
	@@ system will reboot
outer_exc_handled:
	bl _init_platform_variables
	bl _init_variables
	bl _init_hardware
	bl _init_flash
	bl _init_dict
	bl _init_flash_buffers
	bl _do_init
        bl _do_welcome
	bl _quit

_init_platform_variables:
	movs r0, #0
	ldr r1, =sio_hook
	str r0, [r1, #0]
	str r0, [r1, #4]
	ldr r1, =core_1_launched
	str r0, [r1]
	ldr r1, =begin_core_wait
	str r0, [r1]
	str r0, [r1, #4]
	ldr r1, =core_waited
	str r0, [r1]
	str r0, [r1, #4]
	ldr r1, =hold_core
	str r0, [r1]
	str r0, [r1, #4]
	ldr r0, =150000000
	ldr r1, =sysclk
	str r0, [r1]
        ldr r0, =0
        ldr r1, =psram_size
        str r0, [r1]
	bx lr

	@ Prepare for rebooting
	define_internal_word "pre-reboot", visible_flag
_pre_reboot:
	ldr r0, =0xE000E180 @ NVIC_ICER_Base
	ldr r1, =1 << 25 @ SIO_IRQ_FIFO
	ldr r2, [r0]
	orrs r2, r1
	str r2, [r0]
	ldr r0, =0x40018004 @ PSM_FRCE_OFF
	ldr r1, =1 << 24 @ PSM_FRCE_OFF_PROC1
	ldr r2, [r0]
	orrs r2, r1
	str r2, [r0]
1:	ldr r2, [r0]
	tst r2, r1
	beq 1b
	ldr r2, [r0]
	bics r2, r1
	str r2, [r0]
	ldr r0, =FIFO_ST
	ldr r1, =FIFO_ST_VLD
2:	ldr r2, [r0]
	tst r2, r1
	beq 2b
	ldr r0, =FIFO_RD
	ldr r0, [r0]
	bx lr
	end_inlined

        @@ Reboot (note that this does not clear RAM, but it does clear the RAM
	@@ dictionary
	define_word "reboot", visible_flag
_reboot:
        push {r4, lr}

        push_tos
        ldr tos, =reboot_hook
        ldr tos, [tos]
        bl _execute

        ldr r0, =WATCHDOG_BASE
        ldr r1, =ALIAS_SET
        orrs r0, r1
        ldr r1, =WATCHDOG_CTRL_TRIGGER
        ldr r2, =0
        orrs r1, r2
        str r1, [r0, #WATCHDOG_CTRL]

1:      b 2f
2:      b 1b

	pop {r4, pc}
	end_inlined

	@ Reboot the RP2350 in BOOTSEL mode
	define_word "bootsel", visible_flag
_bootsel:
        push {r4, lr}

        push_tos
        ldr tos, =reboot_hook
        ldr tos, [tos]
        bl _execute

        movs r2, #0
	movs r1, #RT_FLAG_FUNC_ARM_SEC
	ldr r0, ='R | ('B << 8)
	ldrh r3, [r2, #0x16]
	blx r3
	movs r4, r0
        ldr r0, =0x2 @ | 0x0100
	movs r1, #0
        movs r2, #0
        movs r3, #0
	blx r4

        ldr r0, =WATCHDOG_BASE
        ldr r1, =ALIAS_SET
        orrs r0, r1
        ldr r1, =WATCHDOG_CTRL_TRIGGER
        ldr r2, =0
        orrs r1, r2
        str r1, [r0, #WATCHDOG_CTRL]

1:      b 2f
2:      b 1b

        pop {r4, pc}
	end_inlined

	@@ Execute a PAUSE word, if one is set
	define_word "pause", visible_flag
_pause:	ldr r0, =SIO_BASE + 0x000
	ldr r0, [r0]
	lsls r0, r0, #2
	ldr r1, =pause_enabled
	ldr r1, [r1, r0]
	cmp r1, #0
	ble 1f
	ldr r0, =pause_hook
	ldr r0, [r0]
	mov pc, r0
1:	bx lr
	end_inlined

        @ Output a hexadecimal nibble
        define_word "h.1", visible_flag
_h_1:   push {lr}
        movs r0, #0xF
        ands tos, r0
        cmp tos, #9
        bhi 1f
        adds tos, '0
        b 2f
1:      adds tos, 'A - 10
2:      bl _emit
        pop {pc}
        end_inlined

        @ Output a hexadecimal 8-bit value, padded with zeroes
        define_word "h.2", visible_flag
_h_2:   push {lr}
        movs r0, #0xFF
        ands tos, r0
        push_tos
        lsrs tos, tos, #4
        bl _h_1
        bl _h_1
        pop {pc}
        end_inlined

        @ Output a hexadecimal 16-bit value, padded with zeroes
        define_word "h.4", visible_flag
_h_4:   push {lr}
        ldr r0, =0xFFFF
        ands tos, r0
        push_tos
        lsrs tos, tos, #8
        bl _h_2
        bl _h_2
        pop {pc}
        end_inlined

        @ Output a hexadecimal 32-bit value, padded with zeroes
        define_word "h.8", visible_flag
_h_8:   push {lr}
        push_tos
        lsrs tos, tos, #16
        bl _h_4
        bl _h_4
        pop {pc}
        end_inlined

        @ output a hexadecimal 64-bit value, padded with zeroes
        define_word "h.16", visible_flag
_h_16:  push {lr}
        bl _h_8
        bl _h_8
        pop {pc}
        end_inlined
        
	.ltorg
	
	.include "../rp2350/hardware.s"
	.include "../rp2350/flashrom.s"
        .include "../rp2350/psram.s"
	.include "../rp2350/console.s"
	.include "../rp2350/expose.s"
        .include "../common/syntax.s"
	.include "../m33/core.s"
	.include "../m33/divide.s"
	.include "../common/outer.s"
	.include "../common/cond.s"
	.include "../m33/asm.s"
	.include "../common/strings.s"
	.include "../m33/double.s"
	.include "../m33/exception.s"
	.include "../rp2350/multicore.s"
	.include "../common/final.s"
