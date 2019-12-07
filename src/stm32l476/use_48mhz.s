; Copyright (c) 2019, Travis Bemann
; All rights reserved.
; 
; Redistribution and use in source and binary forms, with or without
; modification, are permitted provided that the following conditions are met:
; 
; 1. Redistributions of source code must retain the above copyright notice,
;    this list of conditions and the following disclaimer.
; 
; 2. Redistributions in binary form must reproduce the above copyright notice,
;    this list of conditions and the following disclaimer in the documentation
;    and/or other materials provided with the distribution.
; 
; 3. Neither the name of the copyright holder nor the names of its
;    contributors may be used to endorse or promote products derived from
;    this software without specific prior written permission.
; 
; THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
; AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
; IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
; ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
; LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
; CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
; SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
; INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
; CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
; ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
; POSSIBILITY OF SUCH DAMAGE.	

	;; Power management registers
	.equ PWR_BASE, 0x40007000
	.equ PWR_CR1, PWR_BASE + 0x00
	.equ PWR_SR2, PWR_BASE + 0x14
	.equ PWR_CR1_VOS, 0x600
	.equ PWR_CR1_VOS_MODE1, 0x0200
	.equ PWR_SR2_VOSF, 0x0400
	
	;; Flash registers
	.equ FLASH_BASE, 0x40022000
	.equ FLASH_ACR, FLASH_BASE + 0x00
	.equ FLASH_ACR_LATENCY, 0x07
	.equ FLASH_ACR_2WAIT_PREFETCH, 0x0102

	;; RCC registers
	.equ RCC_BASE, 0x40021000
	.equ RCC_CR, RCC_BASE + 0x00
	.equ RCC_CR_MSIRGSEL, 0x08
	.equ RCC_CR_MSIRANGE, 0xF0
	.equ RCC_CR_MSIRANGE_48MHZ, 0xB0
	
	;; Set system clock to 48 MHz MSI and set the flash latency accordingly
	define_word "use-48mhz", visible_flag
use_48mhz:
	bl set_pwr_for_48mhz
	bl set_flash_latency_for_48mhz
	bl set_msi_48mhz
	bx lr

	define_word "set-pwr-for-48mhz", visible_flag
set_pwr_for_48hz:	
	;; Set up the voltage scale
	ldr r0, =PWR_CR1
	ldr r1, [r0]
	bic r1, #PWR_CR1_VOS
	orr r1, #PWR_CR1_MODE1
	str r1, [r0]
	;; Wait for it to become ready
	ldr r0, =PWR_SR2
1:	ldr r1, [r0]
	ands r1, #PWR_SR2_VOSF
	bne 1b
	bx lr

	;; Set the flash latency and prefetch for 48Mhz MSI
	define_word "set-flash-latency-for-48mhz", visible_flag
set_flash_latency_for_48mhz:
	ldr r0, =FLASH_ACR
	ldr r1, [r0]
	bic r1, #FLASH_ACR_LATENCY
	orr r1, #FLASH_ACR_2WAIT_PREFETCH
	str r1, [r0]
	bx lr

	;; Set 48Mhz clock mode
	define_word "set-msi-48mhz", visible_flag
set_msi_48mhz:
	ldr r0, =RCC_CR
	ldr r1, [r0]
	bic r1, #RCC_CR_MSIRANGE
	orr r1, #RCC_CR_MSIRANGE_48MHZ
	str r1, [r0]
	orr r1, #RCC_CR_MSIRGSEL
	str r1, [r0]
	bx lr
