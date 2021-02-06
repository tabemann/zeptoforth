@ Copyright (c) 2013 Matthias Koch
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
	
	.equ GPIOB_Base      ,   0x40020400
	.equ GPIOB_MODER     ,   GPIOB_Base + 0x00
	.equ GPIOB_OTYPER    ,   GPIOB_Base + 0x04
	.equ GPIOB_OSPEEDR   ,   GPIOB_Base + 0x08
	.equ GPIOB_PUPDR     ,   GPIOB_Base + 0x0C
	.equ GPIOB_IDR       ,   GPIOB_Base + 0x10
	.equ GPIOB_ODR       ,   GPIOB_Base + 0x14
	.equ GPIOB_BSRR      ,   GPIOB_Base + 0x18
	.equ GPIOB_LCKR      ,   GPIOB_Base + 0x1C
	.equ GPIOB_AFRL      ,   GPIOB_Base + 0x20
	.equ GPIOB_AFRH      ,   GPIOB_Base + 0x24

	.equ GPIOK_Base      ,   0x40022800
	.equ GPIOK_MODER     ,   GPIOK_Base + 0x00
	
        .equ USART1_Base     ,   0x40011000

	.equ CONSOLE_Base    ,   USART1_Base
        
	.equ CONSOLE_CR1     ,   CONSOLE_Base + 0x00
	.equ CONSOLE_CR2     ,   CONSOLE_Base + 0x04
	.equ CONSOLE_CR3     ,   CONSOLE_Base + 0x08
	.equ CONSOLE_BRR     ,   CONSOLE_Base + 0x0C
	.equ CONSOLE_GTPR    ,   CONSOLE_Base + 0x10
	.equ CONSOLE_RTOR    ,   CONSOLE_Base + 0x14
	.equ CONSOLE_RQR     ,   CONSOLE_Base + 0x18
	.equ CONSOLE_ISR     ,   CONSOLE_Base + 0x1C
	.equ CONSOLE_ICR     ,   CONSOLE_Base + 0x20
	.equ CONSOLE_RDR     ,   CONSOLE_Base + 0x24
	.equ CONSOLE_TDR     ,   CONSOLE_Base + 0x28

	.equ CONSOLE_CR1_UE  ,   0x01
	.equ CONSOLE_CR1_RE  ,   0x04
	.equ CONSOLE_CR1_TE  ,   0x08

	.equ CONSOLE_CR3_OVRDIS , 0x1000
	
        .equ RXNE            ,   0x20
        .equ TC              ,   0x40
        .equ TXE             ,   0x80
	
	@@ Initialize UART
	define_internal_word "uart-init", visible_flag
_uart_init:
	push {lr}

	@ Enable all GPIO peripheral clocks
	ldr r1, =RCC_AHB1ENR
	ldr r0, [r1]
	ldr r2, =0x7FF
	orrs r0, r2
	str r0, [r1]
  
	@ shutdown display illumination for now
	ldr r1, =GPIOK_MODER
	ldr r0, =0x80
	str r0, [r1]  

        @ Set PORTA pins in alternate function mode
        ldr r1, =GPIOA_MODER
        ldr r0, [r1]
	and r0, #0xFFF3FFFF @ PA9
        orrs r0, #0x00080000 @ AF mode
        str r0, [r1]

	@ Set PORTB ports in alternate function mode
	ldr r1, =GPIOB_MODER
	ldr r0, [r1]
	and r0, #0xFFFF3FFF @ PB7
	orrs r0, #0x00008000 @ AF mode
	str r0, [r1]

        @ Set alternate function 7 to enable USART1 pins on Port A
        ldr r1, =GPIOA_AFRH
	ldr r0, [r1]
	and r0, #0xFFFFFF0F
        orrs r0, #0x70              @ Alternate function 7 for TX and RX pins of CONSOLE on PORTA 
        str r0, [r1]

        @ Set alternate function 7 to enable USART1 pins on Port B
        ldr r1, =GPIOB_AFRL
	ldr r0, [r1]
        and r0, #0x0FFFFFFF
        orrs r0, #0x70000000        @ Alternate function 7 for TX and RX pins of CONSOLE on PORTA
        str r0, [r1]

        @ Enable the USART1 peripheral clock by setting bit 4
        ldr r1, =RCC_APB2ENR
	ldr r0, [r1]
        orrs r0, #0x10
        str r0, [r1]

	@ Configure BRR by deviding the bus clock with the baud rate
	ldr r1, =CONSOLE_BRR
	ldr r0, =0x3AA  @ (108000000 + 115200 / 2) / 115200
	str r0, [r1]

	@ Disable USART overrun detection before UE enable
	ldr r1, =CONSOLE_CR3
	ldr r0, =CONSOLE_CR3_OVRDIS
	str r0, [r1]

	@ Enable the USART, TX, and RX circuit
	ldr r1, =CONSOLE_CR1
	ldr r0, =CONSOLE_CR1_UE | CONSOLE_CR1_TE | CONSOLE_CR1_RE
	str r0, [r1]
	
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
	pop {pc}
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
	ldr r0, =CONSOLE_ISR
	ldr r1, [r0]
	movs r0, #RXNE
	ands r1, r0
	beq 1f
	movs tos, #-1
1:	pop {pc}
	end_inlined
	
	.ltorg
	
