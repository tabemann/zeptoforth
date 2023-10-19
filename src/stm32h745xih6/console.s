@ Copyright (c) 2013 Matthias Koch
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
	
        @ Set USART3 to use the HSI clock at 64 MHz
_config_usart3_hsi:
        ldr r0, =RCC_D2CCIP2R
        ldr r1, =RCC_USART234578_CLKSOURCE_HSI
        str r1, [r0]
        bx lr
        end_inlined

        @ Enable GPIO clocks
_enable_gpio_clocks:
        set_bits RCC_AHB4ENR, 0x7FF
        bx lr
        end_inlined

        @ Set alternate function mode 7 for PD8 and PD9
_config_af_pd8_pd9:
        clear_set_bits GPIOD_MODER, 0x00030000, 0x00020000 @ AF for PD8
        clear_set_bits GPIOD_MODER, 0x000C0000, 0x00080000 @ AF for PD9
        clear_set_bits GPIOD_AFRH, 0x0000000F, 0x00000007 @ AF 7 on PD8
        clear_set_bits GPIOD_AFRH, 0x000000F0, 0x00000070 @ AF 7 on PD9
        bx lr
        end_inlined

        @ Enable the USART3 clock
_enable_usart3_clock:
        set_bits RCC_APB1ENR, 0x00040000
        bx lr
        end_inlined

        @ Configure the console USART
_config_console_usart:
        ldr r0, =Console_BRR
        ldr r1, =0x22C @ (64000000 + (115200 / 2)) / 115200
        str r1, [r0]
        set_bits Console_CR3, USART_CR3_OVDIS
        set_bits Console_CR1, (USART_CR1_UE | USART_CR1_TE | USART_CR1_RE)
        bx lr
        end_inlined
        
	@@ Initialize UART
	define_internal_word "uart-init", visible_flag
_uart_init:
	push {lr}
        bl _config_usart3_hsi
        bl _enable_gpio_clocks
        bl _config_af_pd8_pd9
        bl _enable_usart3_clock
        bl _config_console_usart
	pop {pc}
	end_inlined

	@@ Emit one character ( c -- )
	define_internal_word "serial-emit", visible_flag
_serial_emit:
	push {lr}
1:	bl _serial_emit_q
	cmp tos, #0
	pull_tos
	beq 1b
	ldr r2, =CONSOLE_TDR
	strb tos, [r2]         @ Output the character
	pull_tos
	pop {pc}
	end_inlined

	@@ Receive one character ( -- c )
	define_internal_word "serial-key", visible_flag
_serial_key:
	push {lr}
1:	bl _serial_key_q
	cmp tos, #0
	pull_tos
	beq 1b
	push_tos
	ldr r2, =CONSOLE_RDR
	ldrb tos, [r2]
        ldr r0, =uart_special_enabled
        ldr r0, [r0]
        cmp r0, #0
        beq 2f
        cmp tos, #CTRL_C
        bne 2f
        bl _reboot
2:      pop {pc}
	end_inlined

	@@ Test whether a character may be emitted ( -- flag )
	define_internal_word "serial-emit?", visible_flag
_serial_emit_q:
	push {lr}
	bl _pause
	push_tos
	movs tos, #0
	ldr r0, =CONSOLE_ISR
	ldr r1, [r0]
	movs r0, #USART_ISR_TXE
	ands r1, r0
	beq 1f
	movs tos, #-1
1:	pop {pc}
	end_inlined

	@@ Test whether a character is ready be received ( -- flag )
	define_internal_word "serial-key?", visible_flag
_serial_key_q:
	push {lr}
	bl _pause
	push_tos
	movs tos, #0
	ldr r0, =CONSOLE_ISR
	ldr r1, [r0]
	movs r0, #USART_ISR_RXNE
	ands r1, r0
	beq 1f
	movs tos, #-1
1:	pop {pc}
	end_inlined
	
	.ltorg
	
