@ Copyright (c) 2013 Matthias Koch
@ Copyright (c) 2019 Travis Bemann
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
	.equ RCC_CR_MSIRGSEL, 0x08
	.equ RCC_CR_MSIRANGE, 0xF0
	.equ RCC_CR_MSIRANGE_48MHZ, 0xB0
	
	@@ Set system clock to 48 MHz MSI and set the flash latency accordingly
	define_word "use-48mhz", visible_flag
_use_48mhz:
	push {lr}
	bl _set_pwr_for_48mhz
	bl _set_flash_latency_for_48mhz
	bl _set_msi_48mhz
	pop {pc}

	define_word "set-pwr-for-48mhz", visible_flag
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

	@@ Set the flash latency and prefetch for 48Mhz MSI
	define_word "set-flash-latency-for-48mhz", visible_flag
_set_flash_latency_for_48mhz:
	ldr r0, =FLASH_ACR
	ldr r1, [r0]
	movs r2, #FLASH_ACR_LATENCY
	bics r1, r2
	movs r2, #FLASH_ACR_2WAIT_PREFETCH
	orrs r1, r2
	str r1, [r0]
	bx lr

	@@ Set 48Mhz clock mode
	define_word "set-msi-48mhz", visible_flag
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

	@@ Time multiplier
	define_word "time-multiplier", visible_flag
_time_multiplier:
	push_tos
	movs tos, #48
	bx lr

	@@ Time divisor
	define_word "time-divisor", visible_flag
_time_divisor:
	push_tos
	movs tos, #8
	bx lr

	@@ Divisor to get ms from systicks
	define_word "systick-divisor", visible_flag
_systick_divisor:
	push_tos
	movs tos, #10
	bx lr
	.ltorg
	
