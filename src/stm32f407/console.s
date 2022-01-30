@ Copyright (c) 2013 Matthias Koch
@ Copyright (c) 2020-2022 Travis Bemann
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
	
	.equ GPIOA_Base      ,   0x40020000
	.equ GPIOA_MODER     ,   GPIOA_Base + 0x00
	.equ GPIOA_OTYPER    ,   GPIOA_Base + 0x04
	.equ GPIOA_OSPEEDR   ,   GPIOA_Base + 0x08
	.equ GPIOA_PUPDR     ,   GPIOA_Base + 0x0C
	.equ GPIOA_IDR       ,   GPIOA_Base + 0x10
	.equ GPIOA_ODR       ,   GPIOA_Base + 0x14
	.equ GPIOA_BSRR      ,   GPIOA_Base + 0x18
	.equ GPIOA_LCKR      ,   GPIOA_Base + 0x1C
	.equ GPIOA_AFRL      ,   GPIOA_Base + 0x20
	.equ GPIOA_AFRH      ,   GPIOA_Base + 0x24
	
	.equ RCC_Base        ,   0x40023800
	.equ RCC_CR          ,   RCC_Base + 0x00
	.equ RCC_PLLCRGR     ,   RCC_Base + 0x04
	.equ RCC_CFGR        ,   RCC_Base + 0x08
	.equ RCC_AHB1ENR     ,   RCC_Base + 0x30
	.equ RCC_APB1ENR     ,   RCC_Base + 0x40
	
	.equ HSERDY          ,   0x020000
	.equ HSEON           ,   0x010000
        
        .equ USART2_Base     ,   0x40004400

	.equ CONSOLE_Base    ,   USART2_Base
        
	.equ CONSOLE_SR      ,   CONSOLE_Base + 0x00
	.equ CONSOLE_DR      ,   CONSOLE_Base + 0x04
	.equ CONSOLE_BRR     ,   CONSOLE_Base + 0x08
	.equ CONSOLE_CR1     ,   CONSOLE_Base + 0x0c
	.equ CONSOLE_CR2     ,   CONSOLE_Base + 0x10
	.equ CONSOLE_CR3     ,   CONSOLE_Base + 0x14
	.equ CONSOLE_GTPR    ,   CONSOLE_Base + 0x18

	.equ FLASH_ACR       ,   0x40023C00

        .equ RXNE            ,   0x20
        .equ TC              ,   0x40
        .equ TXE             ,   0x80

	.equ PLLON           ,   1 << 24
	.equ PLLRDY          ,   1 << 25
	.equ PLLSRC          ,   1 << 22
	
	@@ Initialize UART
	define_internal_word "uart-init", visible_flag
_uart_init:
	push {lr}
	
        ldr r1, = RCC_CR
        mov r0, HSEON
        str r0, [r1]            @ turn on the external clock
1:	ldr r0, [r1]
        ands r0, #HSERDY
        beq 1b            @ hang here until external clock is stable

        @ at this point, the HSE is running and stable but I suppose we have not yet
        @ switched Sysclk to use it.

        ldr r1, = RCC_CFGR
        mov r0, # 1
        str r0, [r1]            @ switch to the external clock
        
        @ Turn off the HSION bit
        ldr r1, = RCC_CR
        ldr r0, [r1]
        and r0, 0xFFFFFFFE      @ Zero the 0th bit
        str r0, [r1]

        @ Enable the CCM RAM and all GPIO peripheral clock
        ldr r1, = RCC_AHB1ENR
        ldr r0, = 0x1001FF
        str r0, [r1]

        @ Set PORTA pins in alternate function mode
        ldr r1, = GPIOA_MODER
        ldr r0, [r1]
        orrs r0, #0xA0
        str r0, [r1]

        @ Set alternate function 7 to enable USART2 pins on Port A
        ldr r1, = GPIOA_AFRL
        ldr r0, = 0x7700              @ Alternate function 7 for TX and RX pins of CONSOLE on PORTA 
        str r0, [r1]

        @ Enable the USART2 peripheral clock by setting bit 17
        ldr r1, = RCC_APB1ENR
        ldr r0, = 0x20000
        str r0, [r1]

	@ Configure BRR by deviding the bus clock with the baud rate
	
	ldr r1, =CONSOLE_BRR
	@ ldr r0, =0x341  @  9600 bps
	@ movs r0, #0xD0  @ 38400 bps
	movs r0, #0x45  @ 115200 bps
	@ movs r0, #0x46  @ 115200 bps, ein ganz kleines bisschen langsamer...
	str r0, [r1]

	@ Enable 168 MHz
@	bl _use_120mhz
@	bl _use_168mhz
	
	@ Enable the USART, TX, and RX circuit
	ldr r1, =CONSOLE_CR1
	ldr r0, =0x200C @ USART_CR1_UE | USART_CR1_TE | USART_CR1_RE
	str r0, [r1]
	
	pop {pc}
	end_inlined

@	@@ Enable 120 MHz
@	define_internal_word "use-120mhz", visible_flag
@_use_120mhz:
@	ldr r0, =FLASH_ACR
@	ldr r1, =0x103
@	str r1, [r0]
@	ldr r0, =RCC_PLLCRGR
@	ldr r1, =PLLSRC | 8 | (240 << 6) | (8 << 24)
@	str r1, [r0]
@	ldr r0, =RCC_CR
@	ldr r1, [r0]
@	ldr r2, =PLLON
@	orrs r1, r2
@	str r1, [r0]
@	ldr r2, =PLLRDY
@1:	ldr r1, [r0]
@	tst r1, r2
@	beq 1b
@	ldr r0, =RCC_CFGR
@	ldr r1, =2 | (5 << 10) | (5 << 13)
@	str r1, [r0]
@	ldr r0, =CONSOLE_BRR
@	ldr r1, =0x104
@	str r1, [r0]
@	bx lr
@	end_inlined
@
@	@@ Enable 168 MHz
@	define_internal_word "use-168mhz", visible_flag
@_use_168mhz:
@	ldr r0, =FLASH_ACR
@	ldr r1, =0x705
@	str r1, [r0]
@	ldr r0, =RCC_PLLCRGR
@	ldr r1, =PLLSRC | 8 | (336 << 6) | (7 << 24)
@	str r1, [r0]
@	ldr r0, =RCC_CR
@	ldr r1, [r0]
@	ldr r2, =PLLON
@	orrs r1, r2
@	str r1, [r0]
@	ldr r2, =PLLRDY
@1:	ldr r1, [r0]
@	tst r1, r2
@	beq 1b
@	ldr r0, =RCC_CFGR
@	ldr r1, =2 | (5 << 10) | (4 << 13)
@	str r1, [r0]
@	ldr r0, =CONSOLE_BRR
@	ldr r1, =0x16D
@	str r1, [r0]
@	bx lr
@	end_inlined

	@@ Emit one character ( c -- )
	define_internal_word "serial-emit", visible_flag
_serial_emit:
	push {lr}
1:	bl _serial_emit_q
	cmp tos, #0
	pull_tos
	beq 1b
	ldr r2, =CONSOLE_DR
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
	ldr r2, =CONSOLE_DR
	ldrb tos, [r2]
	pop {pc}
	end_inlined

	@@ Test whether a character may be emitted ( -- flag )
	define_internal_word "serial-emit?", visible_flag
_serial_emit_q:
	push {lr}
	bl _pause
	push_tos
	movs tos, #0
	ldr r0, =CONSOLE_SR
	ldr r1, [r0]
	movs r0, #TXE
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
	ldr r0, =CONSOLE_SR
	ldr r1, [r0]
	movs r0, #RXNE
	ands r1, r0
	beq 1f
	movs tos, #-1
1:	pop {pc}
	end_inlined

@	@@ Time multiplier
@	define_internal_word "time-multiplier", visible_flag
@_time_multiplier:
@	push_tos
@@	movs tos, #15 @ 120 MHz
@	movs tos, #21 @ 168 MHz
@	bx lr
@	end_inlined
@
@	@@ Time divisor
@	define_internal_word "time-divisor", visible_flag
@_time_divisor:
@	push_tos
@	movs tos, #1
@	bx lr
@	end_inlined
@
@	@@ Divisor to get ms from systicks
@	define_internal_word "systick-divisor", visible_flag
@_systick_divisor:
@	push_tos
@	movs tos, #10
@	bx lr
@	end_inlined
	
	.ltorg
	
