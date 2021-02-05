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

	.equ RCC_Base        ,   0x40023800
	.equ RCC_CR          ,   RCC_Base + 0x00
	.equ RCC_PLLCFGR     ,   RCC_Base + 0x04
	.equ RCC_CFGR        ,   RCC_Base + 0x08
	.equ RCC_AHB1ENR     ,   RCC_Base + 0x30
	.equ RCC_APB1ENR     ,   RCC_Base + 0x40
	.equ RCC_APB2ENR     ,   RCC_Base + 0x44

	.equ RCC_PWR         ,   1 << 28
	
	.equ RCC_CFGR_HPRE_NODIV, 0
	.equ RCC_CFGR_PPRE_DIV4, 5
	.equ RCC_CFGR_PPRE_DIV2, 4

	.equ RCC_PLLCFGR_PLLM,   0
	.equ RCC_PLLCFGR_PLLN,   6
	.equ RCC_PLLCFGR_PLLP,   16
	.equ RCC_PLLCFGR_PLLQ,   24

	.equ PWR_SCALE1      ,   0

	.equ RCC_CFGR_HPRE_MASK, 15 << 4
	.equ RCC_CFGR_PPRE1_MASK, 7 << 10
	.equ RCC_CFGR_PPRE2_MASK, 7 << 13

	.equ FLASH_ACR_LATENCY, 0

	.equ SET_PLLN        ,   432
	.equ SET_PLLP        ,   2
	.equ SET_PLLQ        ,   9
	.equ SET_HPRE        ,   RCC_CFGR_HPRE_NODIV << 4
	.equ SET_PPRE1       ,   RCC_CFGR_PPRE_DIV4 << 10
	.equ SET_PPRE2       ,   RCC_CFGR_PPRE_DIV2 << 13
	.equ SET_VOS_SCALE   ,   PWR_SCALE1 << 14
	.equ SET_FLASH_WAITS ,   7 << FLASH_ACR_LATENCY

	.equ FLASH_ACR       ,   0x40023C00
	.equ FLASH_ACR_ARTEN ,   0x00000200
	.equ FLASH_ACR_PRFTEN,   0x00000100
	.equ FLASH_ACR_LATENCY_MASK, 15 << 0
	
	.equ RCC_CR_PLLON    ,   1 << 24
	.equ RCC_CR_PLLRDY   ,   1 << 25
	.equ RCC_CR_HSEON    ,   1 << 16
	.equ RCC_CR_HSERDY   ,   1 << 17
	.equ RCC_CR_HSION    ,   1 << 0
	.equ RCC_CR_HSIRDY   ,   1 << 1
	.equ RCC_CFGR_SW_MASK,   3
	.equ RCC_CFGR_SW_HSI ,   0
	.equ RCC_CFGR_SW_PLL ,   2
	.equ RCC_CFGR_SWS_MASK,  3 << 2
	.equ RCC_CFGR_SWS_PLL,   2 << 2

	.equ PWR_Base        ,   0x40007000
	
	.equ PWR_CR1         ,   PWR_Base + 0x00
	.equ PWR_CSR1        ,   PWR_Base + 0x04
	
	.equ PWR_CR1_VOS_MASK,   3 << 14
	.equ PWR_CR1_ODEN    ,   1 << 16
	.equ PWR_CR1_ODSWEN  ,   1 << 17
	.equ PWR_CSR1_ODRDY  ,   1 << 16
	.equ PWR_CSR1_ODSWRDY,   1 << 17
	
	.equ RCC_PLLCFGR_PLLSRC,   1 << 22

	@@ Enable 216 MHz
	define_internal_word "use-216mhz", visible_flag
_use_216mhz:
	ldr r0, =RCC_CR
	ldr r1, [r0]
	ldr r2, =RCC_CR_HSION
	orrs r1, r2
	str r1, [r0]
	ldr r2, =RCC_CR_HSIRDY
1:	ldr r1, [r0]
	tst r1, r2
	beq 1b
	
	ldr r0, =RCC_CFGR
	ldr r1, [r0]
	bics r1, #RCC_CFGR_SW_MASK
	@	orrs r1, #RCC_CFGR_SW_HSI
	str r1, [r0]

	ldr r0, =RCC_CR
	ldr r1, [r0]
	ldr r2, =RCC_CR_HSEON
	orrs r1, r2
	str r1, [r0]
	ldr r2, =RCC_CR_HSERDY
1:	ldr r1, [r0]
	tst r1, r2
	beq 1b

	ldr r0, =RCC_APB1ENR
	ldr r1, [r0]
	ldr r2, =RCC_PWR
	orrs r1, r2
	str r1, [r0]

	ldr r0, =PWR_CR1
	ldr r1, [r0]
	ldr r2, =PWR_CR1_VOS_MASK
	bics r1, r2
	ldr r2, =SET_VOS_SCALE
	orrs r1, r2
	str r1, [r0]

	ldr r1, [r0]
	ldr r2, =PWR_CR1_ODEN
	orrs r1, r2
	str r1, [r0]

	ldr r0, =PWR_CSR1
	ldr r2, =PWR_CSR1_ODRDY
1:	ldr r1, [r0]
	tst r1, r2
	beq 1b

	ldr r0, =PWR_CR1
	ldr r1, [r0]
	ldr r2, =PWR_CR1_ODSWEN
	orrs r1, r2
	str r1, [r0]

	ldr r0, =PWR_CSR1
	ldr r2, =PWR_CSR1_ODSWRDY
1:	ldr r1, [r0]
	tst r1, r2
	beq 1b

	ldr r0, =RCC_CFGR
	ldr r1, [r0]
	ldr r2, =RCC_CFGR_HPRE_MASK
	bics r1, r2
	ldr r2, =SET_HPRE
	orrs r1, r2
	str r1, [r0]

	ldr r0, =RCC_CFGR
	ldr r1, [r0]
	ldr r2, =RCC_CFGR_PPRE1_MASK
	bics r1, r2
	ldr r2, =SET_PPRE1
	orrs r1, r2
	str r1, [r0]

	ldr r0, =RCC_CFGR
	ldr r1, [r0]
	ldr r2, =RCC_CFGR_PPRE2_MASK
	bics r1, r2
	ldr r2, =SET_PPRE2
	orrs r1, r2
	str r1, [r0]

	ldr r0, =RCC_CR
	ldr r1, [r0]
	ldr r2, =RCC_CR_PLLON
	bics r1, r2
	str r1, [r0]

	ldr r0, =RCC_PLLCFGR
	ldr r1, =RCC_PLLCFGR_PLLSRC | (25 << RCC_PLLCFGR_PLLM) | (SET_PLLN << RCC_PLLCFGR_PLLN) | (((SET_PLLP >> 1) - 1) << RCC_PLLCFGR_PLLP) | (SET_PLLQ << RCC_PLLCFGR_PLLQ)
	str r1, [r0]
	
	ldr r0, =RCC_CR
	ldr r1, [r0]
	ldr r2, =RCC_CR_PLLON
	orrs r1, r2
	str r1, [r0]
	
	ldr r2, =RCC_CR_PLLRDY
1:	ldr r1, [r0]
	tst r1, r2
	beq 1b

	ldr r0, =FLASH_ACR
	ldr r1, [r0]
	ldr r2, =FLASH_ACR_LATENCY_MASK
	bics r1, r2
	ldr r2, =SET_FLASH_WAITS
	orrs r1, r2
	str r1, [r0]

	ldr r1, [r0]
	ldr r2, =FLASH_ACR_ARTEN
	orrs r1, r2
	str r1, [r0]

	ldr r1, [r0]
	ldr r2, =FLASH_ACR_PRFTEN
	orrs r1, r2
	str r1, [r0]

	ldr r0, =RCC_CFGR
	ldr r1, [r0]
	bics r1, #RCC_CFGR_SW_MASK
	orrs r1, #RCC_CFGR_SW_PLL
	str r1, [r0]

1:	ldr r1, [r0]
	ands r1, #RCC_CFGR_SWS_MASK
	cmp r1, #RCC_CFGR_SWS_PLL
	bne 1b

	ldr r0, =CONSOLE_BRR
	ldr r1, =0x753 @ (216000000 + 115200 / 2) / 115200
	str r1, [r0]
	bx lr
	end_inlined

	@@ Time multiplier
	define_internal_word "time-multiplier", visible_flag
_time_multiplier:
	push_tos
@	movs tos, #15 @ 120 MHz
	movs tos, #21 @ 168 MHz
	bx lr
	end_inlined

	@@ Time divisor
	define_internal_word "time-divisor", visible_flag
_time_divisor:
	push_tos
	movs tos, #1
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
