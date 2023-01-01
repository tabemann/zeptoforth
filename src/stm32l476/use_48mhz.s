@ Copyright (c) 2013 Matthias Koch
@ Copyright (c) 2019-2023 Travis Bemann
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

	@@ Power management registers
	.equ PWR_BASE, 0x40007000
	.equ PWR_CR1, PWR_BASE + 0x00
	.equ PWR_SR2, PWR_BASE + 0x14
	.equ PWR_CR1_VOS, 0x600
	.equ PWR_CR1_VOS_MODE1, 0x0200
	.equ PWR_SR2_VOSF, 0x0400
	
	@@ Flash registers
	.equ FLASH_BASE, 0x40022000
	.equ FLASH_ACR, FLASH_BASE + 0x00
	.equ FLASH_ACR_LATENCY, 0x07
	.equ FLASH_ACR_2WAIT_PREFETCH, 0x0102

	@@ RCC registers
	.equ RCC_BASE, 0x40021000
	.equ RCC_CR, RCC_BASE + 0x00
	.equ RCC_PLLCFGR, RCC_BASE + 0x0C

	.equ RCC_PLLCFGR_PLLR_Value, 0 << 25 @ PLLR = 2
	.equ RCC_PLLCFGR_PLLREN_Value, 1 << 24
	.equ RCC_PLLCFGR_PLLQ_Value, 0 << 21 @ PLLQ = 2
	.equ RCC_PLLCFGR_PLLQEN_Value, 1 << 20
	.equ RCC_PLLCFGR_PLLN_Value, 16 << 8 @ PLLN = 16
	.equ RCC_PLLCFGR_PLLM_Value, 7 << 8 @ PLLM = 8
	.equ RCC_PLLCFGR_PLLSRC_Value, 1 << 0 @ MSI
	
	.equ RCC_CR_MSIRGSEL, 0x08
	.equ RCC_CR_MSIRANGE, 0xF0
	.equ RCC_CR_MSIRANGE_48MHZ, 0xB0
	.equ RCC_CR_PLLRDY, 1 << 25
	.equ RCC_CR_PLLON, 1 << 24
	
	@@ Set system clock to 48 MHz MSI and set the flash latency accordingly
	define_internal_word "use-48mhz", visible_flag
_use_48mhz:
	push {lr}
	bl _set_pwr_for_48mhz
	bl _set_flash_latency_for_48mhz
	bl _set_msi_48mhz
@	bl _enable_pll
	pop {pc}
	end_inlined

	define_internal_word "set-pwr-for-48mhz", visible_flag
_set_pwr_for_48mhz:	
	@@ Set up the voltage scale
	ldr r0, =PWR_CR1
	ldr r1, [r0]
	movs r2, #PWR_CR1_VOS
	bics r1, r2
	movs r2, #PWR_CR1_VOS_MODE1
	orrs r1, r2
	str r1, [r0]
	@@ Wait for it to become ready
	ldr r0, =PWR_SR2
1:	ldr r1, [r0]
	movs r2, #PWR_SR2_VOSF
	ands r1, r2
	bne 1b
	bx lr
	end_inlined

	@@ Set the flash latency and prefetch for 48Mhz MSI
	define_internal_word "set-flash-latency-for-48mhz", visible_flag
_set_flash_latency_for_48mhz:
	ldr r0, =FLASH_ACR
	ldr r1, [r0]
	movs r2, #FLASH_ACR_LATENCY
	bics r1, r2
	movs r2, #FLASH_ACR_2WAIT_PREFETCH
	orrs r1, r2
	str r1, [r0]
	bx lr
	end_inlined

	@@ Set 48Mhz clock mode
	define_internal_word "set-msi-48mhz", visible_flag
_set_msi_48mhz:
	ldr r0, =RCC_CR
	ldr r1, [r0]
	movs r2, #RCC_CR_MSIRANGE
	bics r1, r2
	movs r2, #RCC_CR_MSIRANGE_48MHZ
	orrs r1, r2
	str r1, [r0]
	movs r2, #RCC_CR_MSIRGSEL
	orrs r1, r2
	str r1, [r0]
	bx lr
	end_inlined

@	@@ Enable the PLL
@	define_internal_word "enable-pll", visible_flag
@_enable_pll:
@	ldr r0, =RCC_CR
@	ldr r1, =RCC_CR_PLLON
@	ldr r2, [r0]
@	bics r2, r1
@	str r2, [r0]
@	ldr r0, =RCC_PLLCFGR
@	ldr r1, =(RCC_PLLCFGR_PLLQ_Value | RCC_PLLCFGR_PLLQEN_Value | RCC_PLLCFGR_PLLN_Value | RCC_PLLCFGR_PLLM_Value | RCC_PLLCFGR_PLLSRC_Value)
@	str r1, [r0]
@	ldr r0, =RCC_CR
@	ldr r1, =RCC_CR_PLLON
@	ldr r2, [r0]
@	orrs r2, r1
@	str r2, [r0]
@	ldr r1, =RCC_CR_PLLRDY
@1:	ldr r2, [r0]
@	tst r2, r1
@	beq 1b
@	bx lr
@	end_inlined
	
	@@ Time multiplier
	define_internal_word "time-multiplier", visible_flag
_time_multiplier:
	push_tos
	movs tos, #48
	bx lr
	end_inlined

	@@ Time divisor
	define_internal_word "time-divisor", visible_flag
_time_divisor:
	push_tos
	movs tos, #8
	bx lr
	end_inlined

	@@ Divisor to get ms from systicks
	define_internal_word "systick-divisor", visible_flag
_systick_divisor:
	push_tos
	movs tos, #10
	bx lr
	end_inlined
	
	.ltorg
	
