@ Copyright (c) 2023 Travis Bemann
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

        @ Configuration and Control Register
        .equ CCR, 0xE000ED14

        @ Cache Level ID Register
        .equ CLIDR, 0xE000ED78

        @ Cache Size ID Register
        .equ CCSIDR, 0xE000ED80

        @ Cache Size Selection Register
        .equ CCSELR, 0xE000ED84
        
        @ Instruction cache invalidate all to Point of Unification (PoU)
        @ Writing 0 to this is sufficient
        .equ ICIALLU, 0xE000EF50

        @ Data cache invalidate by set/way
        @ 
        .equ DCISW, 0xE000EF60

        @ Enable data cache
        @ Or this with the contents of CCR
        .equ CCR_DC, 1 << 16

        @ Enable instruction cache
        @ Or this with the contents of CCR
        .equ CCR_IC, 1 << 17

        .equ CSSELR_IN_D, 0x1

        .equ CCSIDR_LINE_SIZE_Mask, 0x7

        .equ CCSIDR_ASSOCIATIVITY_Shifted_Mask, 0x3FF
        .equ CCSIDR_ASSOCIATIVITY_Shift, 3

        .equ CCSIDR_NUM_SETS_Shifted_Mask, 0x7FFF
        .equ CCSIDR_NUM_SETS_Shift, 13

        @ Flash registers
        .equ FLASH_BASE, 0x52002000
        .equ FLASH_ACR, FLASH_BASE + 0x00

        .equ FLASH_ACR_LATENCY_4, 4
        .equ FLASH_ACR_LATENCY_MASK, 0xF

        @ Voltage scaling
        .equ PWR_BASE, 0x58024800
        .equ PWR_CR3, PWR_BASE + 0x0C
        .equ PWR_D3CR, PWR_BASE + 0x18
        
        .equ PWR_CR3_BYPASS, 1 << 0
        .equ PWR_CR3_LDOEN, 1 << 1
        .equ PWR_CR3_SCUEN, 1 << 2
        .equ PWR_D3CR_VOS_SCALE1, 3 << 14
        .equ PWR_D3CR_VOS_MASK, 3 << 14

        @ RCC registers
	.equ RCC_BASE, 0x58024400
	.equ RCC_CR, RCC_BASE + 0x00
	.equ RCC_CFGR, RCC_BASE + 0x10
	.equ RCC_D1CFGR, RCC_BASE + 0x18
	.equ RCC_D2CFGR, RCC_BASE + 0x1C
	.equ RCC_D3CFGR, RCC_BASE + 0x20
	.equ RCC_PLLCKSELR, RCC_BASE + 0x28
	.equ RCC_PLLCFGR, RCC_BASE + 0x2C
	.equ RCC_PLL1DIVR, RCC_BASE + 0x30
	.equ RCC_D2CCIP2R, RCC_BASE + 0x54
	.equ RCC_AHB4ENR, RCC_BASE + 0xE0
	.equ RCC_APB1ENR, RCC_BASE + 0xE8
	.equ RCC_APB2ENR, RCC_BASE + 0xF0
        .equ RCC_APB1LLPENR, RCC_BASE + 0x110

        .equ RCC_APB1ENR_USART3EN, 1 << 18
        .equ RCC_APB1LLPENR_USART3LPEN, 1 << 18

        .equ RCC_CR_HSEON, 1 << 16
        .equ RCC_CR_HSERDY, 1 << 17
        .equ RCC_CR_HSEBYP, 1 << 18
        .equ RCC_CR_PLL1ON, 1 << 24
        .equ RCC_CR_PLL1RDY, 1 << 25
        .equ RCC_CFGR_SW_MASK, 7 << 0
        .equ RCC_CFGR_SW_SYS_CLKSOURCE_PLL1, 3
        .equ RCC_SYSCLK_DIV_1, 0 << 8 @ 480 MHz / 1
        .equ RCC_D1CFGR_HPRE_MASK, 0xF << 0
        .equ RCC_D1CFGR_HPRE_AHB_DIV_2, 8 << 0 @ 480 MHz / 2
        .equ RCC_D2CFGR_D2PPRE1_MASK, 7 << 4
        .equ RCC_D2CFGR_D2PPRE1_APB1_DIV_2, 2 << 5 @ 240 MHz / 2
        .equ RCC_D2CFGR_D2PPRE2_MASK, 7 << 8
        .equ RCC_D2CFGR_D2PPRE2_APB2_DIV_2, 2 << 9 @ 240 MHz / 2
        .equ RCC_D1CFGR_D1PPRE_MASK, 7 << 4
        .equ RCC_D1CFGR_D1PPRE_APB3_DIV_2, 2 << 5 @ 240 MHz / 2
        .equ RCC_D3CFGR_D3PPRE_MASK, 7 << 4
        .equ RCC_D3CFGR_D3PPRE_APB4_DIV_2, 2 << 5 @ 240 MHz / 2
        .equ RCC_USART16_CLKSOURCE_HSI, 3 << 3 @ 64 MHz
        .equ RCC_USART234578_CLKSOURCE_HSI, 3 << 0 @ 64 MHz

        .equ RCC_PLLCKSELR_PLLSRC_MASK, 3
        .equ RCC_PLLCKSELR_PLLSRC_HSE, 2
        .equ RCC_PLLCFGR_DIVP1EN, 1 << 16
        .equ RCC_PLLCFGR_DIVR1EN, 1 << 18
        .equ RCC_PLLCFGR_PLL1RGE_MASK, 3 << 2
        .equ RCC_PLLCFGR_PLL1RGE_8_16, 3 << 2
        .equ RCC_PLLCFGR_PLL1VCOSEL_MASK, 1 << 1
        .equ RCC_PLLCFGR_PLL1VCOSEL_WIDE, 0 << 1
        .equ RCC_PLLCKSELR_DIVM1_MASK, 0x3F << 4
        .equ RCC_PLLCKSELR_DIVM1_VAL, 1 << 4 @ 8 MHz / 1
        .equ RCC_PLL1DIVR_N1_MASK, 0xFF << 1
        .equ RCC_PLL1DIVR_N1_VAL, 119 << 0 @ 8 MHz * 120
        .equ RCC_PLL1DIVR_P1_MASK, 0x7F << 9
        .equ RCC_PLL1DIVR_P1_VAL, 1 << 9 @ 960 MHz / 2
        .equ RCC_PLL1DIVR_Q1_MASK, 0x7F << 16
        .equ RCC_PLL1DIVR_Q1_VAL, 3 << 16 @ 960 MHz / 4
        .equ RCC_PLL1DIVR_R1_MASK, 0x7F << 24
        .equ RCC_PLL1DIVR_R1_VAL, 1 << 24 @ 960 MHz / 2

        @ GPIO registers
        .equ GPIOA_BASE, 0x58020000 + (0 * 0x400)
        .equ GPIOA_MODER, GPIOA_BASE + 0x00
        .equ GPIOA_OTYPER, GPIOA_BASE + 0x04
        .equ GPIOA_OSPEEDR, GPIOA_BASE + 0x08
        .equ GPIOA_PUPDR, GPIOA_BASE + 0x0C
        .equ GPIOA_IDR, GPIOA_BASE + 0x10
        .equ GPIOA_ODR, GPIOA_BASE + 0x14
        .equ GPIOA_BSRR, GPIOA_BASE + 0x18
        .equ GPIOA_LCKR, GPIOA_BASE + 0x1C
        .equ GPIOA_AFRL, GPIOA_BASE + 0x20
        .equ GPIOA_AFRH, GPIOA_BASE + 0x24

        .equ GPIOB_BASE, GPIOA_BASE + (1 * 0x400)
        .equ GPIOB_MODER, GPIOB_BASE + 0x00
        .equ GPIOB_OTYPER, GPIOB_BASE + 0x04
        .equ GPIOB_OSPEEDR, GPIOB_BASE + 0x08
        .equ GPIOB_PUPDR, GPIOB_BASE + 0x0C
        .equ GPIOB_IDR, GPIOB_BASE + 0x10
        .equ GPIOB_ODR, GPIOB_BASE + 0x14
        .equ GPIOB_BSRR, GPIOB_BASE + 0x18
        .equ GPIOB_LCKR, GPIOB_BASE + 0x1C
        .equ GPIOB_AFRL, GPIOB_BASE + 0x20
        .equ GPIOB_AFRH, GPIOB_BASE + 0x24
  
        .equ GPIOC_BASE, GPIOA_BASE + (2 * 0x400)

        .equ GPIOD_BASE, GPIOA_BASE + (3 * 0x400)
        .equ GPIOD_MODER, GPIOD_BASE + 0x00
        .equ GPIOD_AFRH, GPIOD_BASE + 0x24

        .equ GPIOK_MODER, GPIOA_BASE + (10*0x400))

        @ USART registers
        .equ CONSOLE_USART_Base, 0x40004800 @ USART3
        .equ CONSOLE_CR1, CONSOLE_USART_Base + 0x00
        .equ CONSOLE_CR2, CONSOLE_USART_Base + 0x04
        .equ CONSOLE_CR3, CONSOLE_USART_Base + 0x08
        .equ CONSOLE_BRR, CONSOLE_USART_Base + 0x0C
        .equ CONSOLE_GTPR, CONSOLE_USART_Base + 0x10
        .equ CONSOLE_RTOR, CONSOLE_USART_Base + 0x14
        .equ CONSOLE_RQR, CONSOLE_USART_Base + 0x18
        .equ CONSOLE_ISR, CONSOLE_USART_Base + 0x1C
        .equ CONSOLE_ICR, CONSOLE_USART_Base + 0x20
        .equ CONSOLE_RDR, CONSOLE_USART_Base + 0x24
        .equ CONSOLE_TDR, CONSOLE_USART_Base + 0x28

        .equ USART_CR1_FIFOEN, 1 << 29
        .equ USART_CR1_UE, 1 << 0
        .equ USART_CR1_TE, 1 << 3
        .equ USART_CR1_RE, 1 << 2

        .equ USART_ISR_RXFNE, 1 << 5
        .equ USART_ISR_TC, 1 << 6
        .equ USART_ISR_TXFNF, 1 << 7

        .equ USART_CR3_OVRDIS, 1 << 12
        
        @ Invalidate the instruction cache - this must be done prior to enabling
        @ the caches
_invalidate_instr_cache:
        mov r0, #0x0
        ldr r1, =ICIALLU
        str r0, [r1]
        dsb
        isb
        bx lr
        end_inlined

        @ Invalidate the data cache - this must be done prior to enabling the
        @ caches
_invalidate_data_cache:
        push {r4, r5, r9, r10, lr}
        mov r0, #CSSELR_IN_D @ The reference manual says this should be zero
        ldr r1, =CSSELR
        str r0, [r1] @ Select data cache size
        dsb
        ldr r1, =CCSIDR
        ldr r2, [r1] @ Cache size identification
        and r1, r2, #CCSIDR_LINE_SIZE_Mask @ Number of words in a cache line
        add r5, r1, #0x4
        mov r1, #CCSIDR_ASSOCIATIVITY_Shifted_Mask
        ands r4, r1, r2, lsr #CCSIDR_ASSOCIATIVITY_Shift
        mov r1, #CCSIDR_NUM_SETS_Shifted_Mask
        ands r2, r1, r2, lsr #CCSIDR_NUM_SETS_Shift
        clz r10, r4
        ldr r9, =DCISW
1:      mov r1, r4
2:      lsl r3, r1, r10
        lsl r8, r2, r5
        orrr 3, r3, r8
        str r3, [r9] @ Invalidate data cache line
        subs r1, r1, #0x1
        bge 2b
        subs r2, r2, #0x1
        bge 1b
        dsb
        isb
        pop {r4, r5, r9, r10, pc}
        end_inlined

        @ Disable the caches
_disable_caches:
        clear_bits CCR, CCR_DC | CCR_IC
        dsb
        isb
        bx lr
        end_inlined

        @ Enable the caches
_enable_caches:
        set_bits CCR, CCR_DC | CCR_IC
        dsb
        isb
        bx lr
        end_inlined

        @ Configure the caches
_config_caches: 
        push {lr}
        bl _disable_caches
        bl _invalidate_instr_cache
        bl _invalidate_data_cache
        bl _enable_caches
        pop {pc}
        end_inlined

        @ Set power
_config_power:
        clear_set_bits PWR_CR3, (PWR_CR3_SCUEN | PWR_CR3_BYPASS), PWR_CR3_LDOEN
        bx lr
        end_inlined

        @ Set voltage scaling
_config_voltage_scaling:
        clear_set_bits PWR_D3CR, PWR_D3CR_VOS_MASK, PWR_D3CR_VOS_SCALE1
        bx lr
        end_inlined

        @ Set flash latency
_config_flash_latency:
        clear_set_bits FLASH_ACR, FLASH_ACR_LATENCY_MASK, FLASH_ACR_LATENCY_4
        ldr r0, =FLASH_ACR
        ldr r1, =FLASH_ACR_LATENCY_MASK
        ldr r2, =FLASH_ACR_LATENCY_4
1:      ldr r3, [r0]
        ands r3, r1
        cmp r3, r2
        bne 1b
        bx lr

        @ Set bypass HSE
_config_bypass_hse:
        set_bits RCC_CR, RCC_CR_HSEBYP
        set_bits RCC_CR, RCC_CR_HSEON
        ldr r0, =RCC_CR
        ldr r1, =RCC_CR_HSERDY
1:      ldr r2, [r0]
        ands r2, r1
        beq 1b
        bx lr

        @ Configure PLL for 480 MHz system clock
_config_pll:
        clear_set_bits RCC_PLLCKSELR, RCC_PLLCKSELR_PLLSRC_MASK, RCC_PLLCKSELR_PLLSRC_HSE
        set_bits RCC_PLLCFGR, RCC_PLLCFGR_DIVP1EN
        set_bits RCC_PLLCFGR, RCC_PLLCFGR_DIVR1EN
        clear_set_bits RCC_PLLCFGR, RCC_PLLCFGR_PLL1RGE_MASK, RCC_PLLCFGR_PLL1RGE_8_16
        clear_set_bits RCC_PLLCFGR, RCC_PLLCFGR_PLL1VCOSEL_MASK, RCC_PLLCFGR_PLL1VCOSEL_WIDE
        clear_set_bits RCC_PLLCKSELR, RCC_PLLCKSELR_DIVM1_MASK, RCC_PLLCKSELR_DIVM1_VAL
        clear_set_bits RCC_PLL1DIVR, RCC_PLL1DIVR_N1_MASK, RCC_PLL1DIVR_N1_VAL
        clear_set_bits RCC_PLL1DIVR, RCC_PLL1DIVR_P1_MASK, RCC_PLL1DIVR_P1_VAL
        clear_set_bits RCC_PLL1DIVR, RCC_PLL1DIVR_Q1_MASK, RCC_PLL1DIVR_Q1_VAL
        clear_set_bits RCC_PLL1DIVR, RCC_PLL1DIVR_R1_MASK, RCC_PLL1DIVR_R1_VAL
        bx lr

        @ Enable the PLL
_enable_pll:
        set_bits RCC_CR, RCC_CR_Pll1ON
        ldr r0, =RCC_CR
        ldr r1, =RCC_RC_PLL1RDY
1:      ldr r2, [r0]
        ands r2, r1
        beq 1b
        bx lr

        @ Configure the prescalers
_config_prescalers:
        clear_set_bits RCC_D1CFGR, RCC_D1CFGR_HPRE_MASK, RCC_D1CFGR_HPRE_AHB_DIV_2
        clear_set_bits RCC_CFGR, RCC_CFGR_SW_MASK, RCC_CFGR_SW_SYS_CLKSOURCE, PLL1
        clear_set_bits RCC_D1CFGR, RCC_D1CFGR_HPRE_MASK, RCC_D1CFGR_HPRE_AHB_DIV_2
        clear_set_bits RCC_D1CFGR, RCC_D1CFGR_D1PPRE_MASK, RCC_D1CFGR_D1PPRE_APB3_DIV_2
        clear_set_bits RCC_D2CFGR, RCC_D2CFGR_D2PPRE1_MASK, RCC_D2CFGR_D2PPRE1_APB1_DIV_2
        clear_set_bits RCC_D2CFGR, RCC_D2CFGR_D2PPRE2_MASK, RCC_D2CFGR_D2PPRE2_APB2_DIV_2
        clear_set_bits RCC_D2CFGR, RCC_D3CFGR_D3PPRE_MASK, RCC_D3CFGR_D3PPRE_APB4_DIV_2
        bx lr

        @ Configure the hardware
        define_internal_word "config-hardware", visible_flag
_config_hardware:
        push {lr}
        bl _config_caches
        bl _config_flash_latency
        bl _config_power
        bl _config_voltage_scaling
        bl _config_bypass_hse
        bl _config_pll
        bl _enable_pll
        bl _config_prescalers
        pop {pc}

        .ltorg
        
