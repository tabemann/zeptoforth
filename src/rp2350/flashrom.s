@ Copyright (c) 2013 Matthias Koch
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

        .equ RT_FLAG_FUNC_ARM_SEC, 0x0004
        
	.equ FLASH_CODA_ADDR, flash_main_end - 256
	
	.equ XIP_CTRL_BASE, 0x400C8000
	.equ XIP_CTRL_OFFSET, 0x00

        .equ XIP_CTRL_WRITABLE_M1, 1 << 11
        .equ XIP_CTRL_EN_NONSECURE, 0x00000002
        .equ XIP_CTRL_EN_SECURE, 0x00000001
        
	.equ XIP_QMI_BASE, 0x400D0000

        .equ TIMER_BASE, 0x400B0000
        .equ TIMERAWL, TIMER_BASE + 0x28

        .equ RESET_TIME_US, 40
        
	.equ RAM_BASE, 0x20000000
	.equ FLASH_IMAGE_BASE, 0x10001000
	.equ IMAGE_SIZE, 0x8000
	.equ IO_QSPI_BASE, 0x40030000
        .equ PADS_QSPI_BASE, 0x40040000

        .equ QMI_DIRECT_CSR_OFFSET, 0x00
        .equ QMI_DIRECT_TX_OFFSET, 0x04
        .equ QMI_DIRECT_RX_OFFSET, 0x08
        .equ QMI_M0_TIMING_OFFSET, 0x0C
        .equ QMI_M0_RFMT_OFFSET, 0x10
        .equ QMI_M0_RCMD_OFFSET, 0x14
        .equ QMI_M0_WFMT_OFFSET, 0x18
        .equ QMI_M0_WCMD_OFFSET, 0x1C
        .equ QMI_M1_TIMING_OFFSET, 0x20
        .equ QMI_M1_RFMT_OFFSET, 0x24
        .equ QMI_M1_RCMD_OFFSET, 0x28
        .equ QMI_M1_WFMT_OFFSET, 0x2C
        .equ QMI_M1_WCMD_OFFSET, 0x30

        .equ QMI_DIRECT_CSR_CLKDIV_LSB, 22
        .equ QMI_DIRECT_CSR_RXEMPTY, 1 << 16
        .equ QMI_DIRECT_CSR_TXEMPTY, 1 << 11
        .equ QMI_DIRECT_CSR_AUTO_CS1N, 1 << 7
        .equ QMI_DIRECT_CSR_AUTO_CS0N, 1 << 6
        .equ QMI_DIRECT_CSR_ASSERT_CS1N, 1 << 3
        .equ QMI_DIRECT_CSR_ASSERT_CS0N, 1 << 2
        .equ QMI_DIRECT_CSR_BUSY, 1 << 1
        .equ QMI_DIRECT_CSR_EN, 1 << 0

        .equ QMI_DIRECT_TX_NOPUSH, 1 << 20
        .equ QMI_DIRECT_TX_OE, 1 << 19
        .equ QMI_DIRECT_TX_DWIDTH_8_BITS, 0 << 18
        .equ QMI_DIRECT_TX_DWIDTH_16_BITS, 1 << 18
        .equ QMI_DIRECT_TX_IWIDTH_SINGLE, 0 << 16
        .equ QMI_DIRECT_TX_IWIDTH_DOUBLE, 1 << 16
        .equ QMI_DIRECT_TX_IWIDTH_QUAD, 2 << 16

        .equ QMI_M0_TIMING_COOLDOWN_LSB, 30 @ 2 bits
        .equ QMI_M0_TIMING_PAGEBREAK_LSB, 28 @ 2 bits
        .equ QMI_M0_TIMING_SELECT_SETUP, 1 << 25
        .equ QMI_M0_TIMING_SELECT_HOLD_LSB, 23 @ 2 bits
        .equ QMI_M0_TIMING_MAX_SELECT_LSB, 17 @ 6 bits
        .equ QMI_M0_TIMING_MIN_DESELECT_LSB, 12 @ 5 bits
        .equ QMI_M0_TIMING_RXDELAY_LSB, 8 @ 3 bits
        .equ QMI_M0_TIMING_CLKDIV_LSB, 0 @ 8 bits

        .equ QMI_M0_TIMING_PAGEBREAK_NONE, 0 << QMI_M0_TIMING_PAGEBREAK_LSB
        .equ QMI_M0_TIMING_PAGEBREAK_256_BYTE, 1 << QMI_M0_TIMING_PAGEBREAK_LSB
        .equ QMI_M0_TIMING_PAGEBREAK_1024_BYTE, 2 << QMI_M0_TIMING_PAGEBREAK_LSB
        .equ QMI_M0_TIMING_PAGEBREAK_4096_BYTE, 3 << QMI_M0_TIMING_PAGEBREAK_LSB

        .equ QMI_M1_TIMING_COOLDOWN_LSB, 30 @ 2 bits
        .equ QMI_M1_TIMING_PAGEBREAK_LSB, 28 @ 2 bits
        .equ QMI_M1_TIMING_SELECT_SETUP, 1 << 25
        .equ QMI_M1_TIMING_SELECT_HOLD_LSB, 23 @ 2 bits
        .equ QMI_M1_TIMING_MAX_SELECT_LSB, 17 @ 6 bits
        .equ QMI_M1_TIMING_MIN_DESELECT_LSB, 12 @ 5 bits
        .equ QMI_M1_TIMING_RXDELAY_LSB, 8 @ 3 bits
        .equ QMI_M1_TIMING_CLKDIV_LSB, 0 @ 8 bits

        .equ QMI_M1_TIMING_PAGEBREAK_NONE, 0 << QMI_M1_TIMING_PAGEBREAK_LSB
        .equ QMI_M1_TIMING_PAGEBREAK_256_BYTE, 1 << QMI_M1_TIMING_PAGEBREAK_LSB
        .equ QMI_M1_TIMING_PAGEBREAK_1024_BYTE, 2 << QMI_M1_TIMING_PAGEBREAK_LSB
        .equ QMI_M1_TIMING_PAGEBREAK_4096_BYTE, 3 << QMI_M1_TIMING_PAGEBREAK_LSB

        .equ SINGLE_WIDTH, 0
        .equ DUAL_WIDTH, 1
        .equ QUAD_WIDTH, 2
        .equ WIDTH_MASK, 3

        .equ QMI_M0_RFMT_DTR, 1 << 28
        .equ QMI_M0_RFMT_DUMMY_LEN_LSB, 16
        .equ QMI_M0_RFMT_SUFFIX_LEN_8_BITS, 2 << 14 @ unset for no suffix
        .equ QMI_M0_RFMT_PREFIX_LEN_8_BITS, 1 << 12 @ unset for no prefix
        .equ QMI_M0_RFMT_DATA_WIDTH_LSB, 8 @ 2 bit
        .equ QMI_M0_RFMT_DUMMY_WIDTH_LSB, 6 @ 2 bit
        .equ QMI_M0_RFMT_SUFFIX_WIDTH_LSB, 4 @ 2 bit
        .equ QMI_M0_RFMT_ADDR_WIDTH_LSB, 2 @ 2 bit
        .equ QMI_M0_RFMT_PREFIX_WIDTH_LSB, 0 @ 2 bit

        .equ QMI_M0_RFMT_DUMMY_LEN_0_BITS, 0 << QMI_M0_RFMT_DUMMY_LEN_LSB
        .equ QMI_M0_RFMT_DUMMY_LEN_4_BITS, 1 << QMI_M0_RFMT_DUMMY_LEN_LSB
        .equ QMI_M0_RFMT_DUMMY_LEN_8_BITS, 2 << QMI_M0_RFMT_DUMMY_LEN_LSB
        .equ QMI_M0_RFMT_DUMMY_LEN_12_BITS, 3 << QMI_M0_RFMT_DUMMY_LEN_LSB
        .equ QMI_M0_RFMT_DUMMY_LEN_16_BITS, 4 << QMI_M0_RFMT_DUMMY_LEN_LSB
        .equ QMI_M0_RFMT_DUMMY_LEN_20_BITS, 5 << QMI_M0_RFMT_DUMMY_LEN_LSB
        .equ QMI_M0_RFMT_DUMMY_LEN_24_BITS, 6 << QMI_M0_RFMT_DUMMY_LEN_LSB
        .equ QMI_M0_RFMT_DUMMY_LEN_28_BITS, 7 << QMI_M0_RFMT_DUMMY_LEN_LSB

        .equ QMI_M0_RCMD_SUFFIX_LSB, 8 @ 8 bit
        .equ QMI_M0_RCMD_PREFIX_LSB, 0 @ 8 bit

        .equ QMI_M1_RFMT_DTR, 1 << 28
        .equ QMI_M1_RFMT_DUMMY_LEN_LSB, 16
        .equ QMI_M1_RFMT_SUFFIX_LEN_8_BITS, 2 << 14 @ unset for no suffix
        .equ QMI_M1_RFMT_PREFIX_LEN_8_BITS, 1 << 12 @ unset for no prefix
        .equ QMI_M1_RFMT_DATA_WIDTH_LSB, 8 @ 2 bit
        .equ QMI_M1_RFMT_DUMMY_WIDTH_LSB, 6 @ 2 bit
        .equ QMI_M1_RFMT_SUFFIX_WIDTH_LSB, 4 @ 2 bit
        .equ QMI_M1_RFMT_ADDR_WIDTH_LSB, 2 @ 2 bit
        .equ QMI_M1_RFMT_PREFIX_WIDTH_LSB, 0 @ 2 bit

        .equ QMI_M1_RFMT_DUMMY_LEN_0_BITS, 0 << QMI_M1_RFMT_DUMMY_LEN_LSB
        .equ QMI_M1_RFMT_DUMMY_LEN_4_BITS, 1 << QMI_M1_RFMT_DUMMY_LEN_LSB
        .equ QMI_M1_RFMT_DUMMY_LEN_8_BITS, 2 << QMI_M1_RFMT_DUMMY_LEN_LSB
        .equ QMI_M1_RFMT_DUMMY_LEN_12_BITS, 3 << QMI_M1_RFMT_DUMMY_LEN_LSB
        .equ QMI_M1_RFMT_DUMMY_LEN_16_BITS, 4 << QMI_M1_RFMT_DUMMY_LEN_LSB
        .equ QMI_M1_RFMT_DUMMY_LEN_20_BITS, 5 << QMI_M1_RFMT_DUMMY_LEN_LSB
        .equ QMI_M1_RFMT_DUMMY_LEN_24_BITS, 6 << QMI_M1_RFMT_DUMMY_LEN_LSB
        .equ QMI_M1_RFMT_DUMMY_LEN_28_BITS, 7 << QMI_M1_RFMT_DUMMY_LEN_LSB

        .equ QMI_M1_RCMD_SUFFIX_LSB, 8 @ 8 bit
        .equ QMI_M1_RCMD_PREFIX_LSB, 0 @ 8 bit
        
        .equ QMI_M1_WFMT_DTR, 1 << 28
        .equ QMI_M1_WFMT_DUMMY_LEN_LSB, 16
        .equ QMI_M1_WFMT_SUFFIX_LEN_8_BITS, 2 << 14 @ unset for no suffix
        .equ QMI_M1_WFMT_PREFIX_LEN_8_BITS, 1 << 12 @ unset for no prefix
        .equ QMI_M1_WFMT_DATA_WIDTH_LSB, 8 @ 2 bit
        .equ QMI_M1_WFMT_DUMMY_WIDTH_LSB, 6 @ 2 bit
        .equ QMI_M1_WFMT_SUFFIX_WIDTH_LSB, 4 @ 2 bit
        .equ QMI_M1_WFMT_ADDR_WIDTH_LSB, 2 @ 2 bit
        .equ QMI_M1_WFMT_PREFIX_WIDTH_LSB, 0 @ 2 bit

        .equ QMI_M1_WFMT_DUMMY_LEN_0_BITS, 0 << QMI_M1_WFMT_DUMMY_LEN_LSB
        .equ QMI_M1_WFMT_DUMMY_LEN_4_BITS, 1 << QMI_M1_WFMT_DUMMY_LEN_LSB
        .equ QMI_M1_WFMT_DUMMY_LEN_8_BITS, 2 << QMI_M1_WFMT_DUMMY_LEN_LSB
        .equ QMI_M1_WFMT_DUMMY_LEN_12_BITS, 3 << QMI_M1_WFMT_DUMMY_LEN_LSB
        .equ QMI_M1_WFMT_DUMMY_LEN_16_BITS, 4 << QMI_M1_WFMT_DUMMY_LEN_LSB
        .equ QMI_M1_WFMT_DUMMY_LEN_20_BITS, 5 << QMI_M1_WFMT_DUMMY_LEN_LSB
        .equ QMI_M1_WFMT_DUMMY_LEN_24_BITS, 6 << QMI_M1_WFMT_DUMMY_LEN_LSB
        .equ QMI_M1_WFMT_DUMMY_LEN_28_BITS, 7 << QMI_M1_WFMT_DUMMY_LEN_LSB

        .equ QMI_M1_WCMD_SUFFIX_LSB, 8 @ 8 bit
        .equ QMI_M1_WCMD_PREFIX_LSB, 0 @ 8 bit

        .equ INIT_XIP_QMI_M0_RFMT, (SINGLE_WIDTH << QMI_M0_RFMT_PREFIX_WIDTH_LSB) | (QUAD_WIDTH << QMI_M0_RFMT_ADDR_WIDTH_LSB) | (QUAD_WIDTH << QMI_M0_RFMT_SUFFIX_WIDTH_LSB) | (QUAD_WIDTH << QMI_M0_RFMT_DUMMY_WIDTH_LSB) | (QUAD_WIDTH << QMI_M0_RFMT_DATA_WIDTH_LSB) | QMI_M0_RFMT_PREFIX_LEN_8_BITS | QMI_M0_RFMT_SUFFIX_LEN_8_BITS | QMI_M0_RFMT_DUMMY_LEN_16_BITS

        .equ INIT_XIP_QMI_M0_RCMD, (CMD_CONT_READ << QMI_M0_RCMD_SUFFIX_LSB) | (CMD_READ_DATA_FAST_QUAD_IO << QMI_M0_RCMD_PREFIX_LSB)

        .equ CONT_XIP_QMI_M0_RFMT, (SINGLE_WIDTH << QMI_M0_RFMT_PREFIX_WIDTH_LSB) | (QUAD_WIDTH << QMI_M0_RFMT_ADDR_WIDTH_LSB) | (QUAD_WIDTH << QMI_M0_RFMT_SUFFIX_WIDTH_LSB) | (QUAD_WIDTH << QMI_M0_RFMT_DUMMY_WIDTH_LSB) | (QUAD_WIDTH << QMI_M0_RFMT_DATA_WIDTH_LSB) | QMI_M0_RFMT_SUFFIX_LEN_8_BITS | QMI_M0_RFMT_DUMMY_LEN_16_BITS

        .equ CONT_XIP_QMI_M0_RCMD, CMD_CONT_READ << QMI_M0_RCMD_SUFFIX_LSB

        .equ CMD_READ_DATA_FAST_QUAD_IO_W_OPTS, CMD_READ_DATA_FAST_QUAD_IO | QMI_DIRECT_TX_IWIDTH_QUAD | QMI_DIRECT_TX_OE | QMI_DIRECT_TX_NOPUSH

        .equ CMD_CONT_READ_W_OPTS, CMD_CONT_READ | QMI_DIRECT_TX_IWIDTH_QUAD | QMI_DIRECT_TX_OE | QMI_DIRECT_TX_NOPUSH
        
	@ Commands
	.equ CMD_WRITE_STATUS, 0x01
	.equ CMD_READ_DATA_FAST_QUAD_IO, 0xEB @
	.equ CMD_READ_STATUS, 0x05
	.equ CMD_WRITE_ENABLE, 0x06
	.equ CMD_READ_STATUS_2, 0x35
	.equ CMD_CONT_READ, 0xA0
	.equ CMD_SECTOR_ERASE, 0x20
	.equ CMD_BLOCK_ERASE_32K, 0x52
	.equ CMD_BLOCK_ERASE_64K, 0xD8
	.equ CMD_PAGE_PROGRAM, 0x02
        .equ CMD_ENABLE_RESET, 0x66
        .equ CMD_RESET, 0x99
	.equ CMD_READ_UUID, 0x4B

	@ QSPI Enable state
	.equ QSPI_ENABLE_STATE, 0x02

	@ Write busy state
	.equ WRITE_BUSY_STATE, 0x01

	@ Wait cycles
	.equ WAIT_CYCLES, 4

	@ Pad control constants
	.equ PADS_QSPI_GPIO_QSPI_SCLK_DRIVE_LSB, 4
	.equ PADS_QSPI_GPIO_QSPI_SCLK_SLEWFAST_BITS, 0x01
	.equ PADS_QSPI_GPIO_QSPI_SCLK_OFFSET, 0x04
	.equ PADS_QSPI_GPIO_QSPI_SD0_OFFSET, 0x08
	.equ PADS_QSPI_GPIO_QSPI_SD1_OFFSET, 0x0C
	.equ PADS_QSPI_GPIO_QSPI_SD2_OFFSET, 0x10
	.equ PADS_QSPI_GPIO_QSPI_SD3_OFFSET, 0x14
	.equ PADS_QSPI_GPIO_QSPI_SD0_SCHMITT_BITS, 0x02

	@ QSPI page size
	.equ QSPI_PAGE_SIZE, 256

        @ Initial pad SCLK configuration, 8mA drive, no slew limiting,
        @ input buffer disabled
        .equ INIT_PAD_SCLK, (2 << PADS_QSPI_GPIO_QSPI_SCLK_DRIVE_LSB) | PADS_QSPI_GPIO_QSPI_SCLK_SLEWFAST_BITS

        @ Initial direct CSR configuration, 5 MHz and 150 MHz clk_sys
        .equ INIT_DIRECT_CSR, (30 << QMI_DIRECT_CSR_CLKDIV_LSB) | QMI_DIRECT_CSR_EN

        .equ PADS_QSPI_GPIO_QSPI_SD0_OFFSET, 0x08
	.equ PADS_QSPI_GPIO_QSPI_SD1_OFFSET, 0x0C
	.equ PADS_QSPI_GPIO_QSPI_SD2_OFFSET, 0x10
	.equ PADS_QSPI_GPIO_QSPI_SD3_OFFSET, 0x14
        .equ PADS_QSPI_GPIO_QSPI_SD0_ISO_BITS, 1 << 8
	.equ PADS_QSPI_GPIO_QSPI_SD0_SCHMITT_BITS, 0x02
        
        @ Initial M0 timing configuratiton
        .equ INIT_M0_TIMING, (1 << QMI_M0_TIMING_COOLDOWN_LSB) | (2 << QMI_M0_TIMING_RXDELAY_LSB) | (4 << QMI_M0_TIMING_CLKDIV_LSB)

	@ Erase flash up to a specified address if needed
	define_internal_word "init-flash", visible_flag
_init_flash:
	push {lr}

        ldr r0, =PADS_QSPI_BASE
        ldr r1, =INIT_PAD_SCLK
        str r1, [r0, #PADS_QSPI_GPIO_QSPI_SCLK_OFFSET]
        ldr r0, =PADS_QSPI_BASE | ALIAS_CLR
        ldr r1, =PADS_QSPI_GPIO_QSPI_SD0_SCHMITT_BITS
        str r1, [r0, #PADS_QSPI_GPIO_QSPI_SD0_OFFSET]
        str r1, [r0, #PADS_QSPI_GPIO_QSPI_SD1_OFFSET]
        str r1, [r0, #PADS_QSPI_GPIO_QSPI_SD2_OFFSET]
        str r1, [r0, #PADS_QSPI_GPIO_QSPI_SD3_OFFSET]

        ldr r0, =XIP_QMI_BASE
        ldr r1, =INIT_M0_TIMING
        str r1, [r0, #QMI_M0_TIMING_OFFSET]

        dmb
        dsb
        isb

        ldr r0, =flash_start
        ldr r0, [r0]
        dmb
        dsb
        isb

        bl _reset_flash

        @ Test
        ldr r0, =flash_start
        ldr r0, [r0]
        dmb
        dsb
        isb

	ldr r0, =FLASH_CODA_ADDR
	ldr r1, =0xFFFFFFFF
	ldr r0, [r0]
	cmp r0, r1
	beq 1f

        push_tos
	movs tos, r0
        push_tos

@        push_tos
@        movs tos, 0x0D
@        bl _emit
@        push_tos
@        movs tos, 0x0A
@        bl _emit
@        push_tos
@        bl _h_8

        ldr tos, =flash_dict_end - flash_start
	bl _erase_range

        cpsie i
        bl _release_core
        push_tos
        ldr tos, =FLASH_CODA_ADDR
        bl _erase_qspi_4k_sector
1:

        pop {pc}
	end_inlined

_reset_flash:
	push {lr}

	bl _force_core_wait

        cpsid i
        dsb
        isb

        bl _exit_xip
	bl _enable_flash_cmd
        bl _force_flash_cs_low
        ldr r0, =XIP_QMI_BASE
        ldr r1, =CMD_ENABLE_RESET | QMI_DIRECT_TX_NOPUSH
        str r1, [r0, #QMI_DIRECT_TX_OFFSET]
        bl _wait_qmi_busy
        bl _force_flash_cs_high
        b 1f
1:      b 1f
1:      b 1f
1:      b 1f
1:      b 1f
1:      b 1f
1:      b 1f
1:      b 1f
1:      bl _force_flash_cs_low
        ldr r0, =XIP_QMI_BASE
        ldr r1, =CMD_RESET | QMI_DIRECT_TX_NOPUSH
        str r1, [r0, #QMI_DIRECT_TX_OFFSET]
        bl _wait_qmi_busy
        bl _force_flash_cs_high
        ldr r0, =TIMERAWL
        ldr r1, [r0]
        adds r1, #RESET_TIME_US
1:      ldr r2, [r0]
        cmp r1, r2
        bgt 1b        

	@ Read the unique ID
        bl _force_flash_cs_low
        ldr r0, =XIP_QMI_BASE
        movs r1, #CMD_READ_UUID
        str r1, [r0, #QMI_DIRECT_TX_OFFSET]
        bl _wait_qmi_busy
	ldr r0, =XIP_QMI_BASE
        ldr r1, [r0, #QMI_DIRECT_RX_OFFSET]
	movs r3, #12
	rsbs r3, #0
2:	ldr r0, =XIP_QMI_BASE
	str r1, [r0, #QMI_DIRECT_TX_OFFSET]
	push {r3}
        bl _wait_qmi_busy
	pop {r3}
	ldr r0, =XIP_QMI_BASE
        ldr r1, [r0, #QMI_DIRECT_RX_OFFSET]
	ldr r2, =pico_uuid+12
	strb r1, [r2, r3]
	adds r3, #1
	bmi 2b
        
        bl _force_flash_cs_high
	bl _enable_flush_xip_cache
	bl _enter_xip
	cpsie i
        bl _release_core
        pop {pc}
	end_inlined

	@ Force the CS pin HIGH
	define_word "force-flash-cs-high", visible_flag
_force_flash_cs_high:
        push {lr}
        bl _wait_qmi_busy
        ldr r0, =XIP_QMI_BASE
        ldr r2, [r0, #QMI_DIRECT_CSR_OFFSET]
        ldr r1, =QMI_DIRECT_CSR_AUTO_CS0N | QMI_DIRECT_CSR_ASSERT_CS0N
        bics r2, r1
        str r2, [r0, #QMI_DIRECT_CSR_OFFSET]
        pop {pc}
	end_inlined

	@ Force the CS pin LOW
	define_word "force-flash-cs-low", visible_flag
_force_flash_cs_low:
        push {lr}
        bl _wait_qmi_busy
        ldr r0, =XIP_QMI_BASE
        ldr r2, [r0, #QMI_DIRECT_CSR_OFFSET]
        ldr r1, =QMI_DIRECT_CSR_AUTO_CS0N
        bics r2, r1
        ldr r1, =QMI_DIRECT_CSR_ASSERT_CS0N
        orrs r2, r1
        str r2, [r0, #QMI_DIRECT_CSR_OFFSET]
        pop {pc}
	end_inlined

	@ Force the CS pin NORMAL
	define_word "force-flash-cs-normal", visible_flag
_force_flash_cs_normal:
        push {lr}
        bl _wait_qmi_busy
        ldr r0, =XIP_QMI_BASE
        ldr r2, [r0, #QMI_DIRECT_CSR_OFFSET]
        ldr r1, =QMI_DIRECT_CSR_AUTO_CS0N
        orrs r2, r1
        ldr r1, =QMI_DIRECT_CSR_ASSERT_CS0N
        bics r2, r1
        str r2, [r0, #QMI_DIRECT_CSR_OFFSET]
        pop {pc}
	end_inlined

	define_word "enable-flash-cmd", visible_flag
_enable_flash_cmd:
	push {lr}
	ldr r0, =XIP_QMI_BASE
	ldr r1, =INIT_DIRECT_CSR
	str r1, [r0, #QMI_DIRECT_CSR_OFFSET]
        bl _wait_qmi_busy
	pop {pc}
	end_inlined

	@ Write an address to flash ( addr cmd -- )
	define_word "write-flash-address", visible_flag
_write_flash_address:
	push {lr}
	bl _force_flash_cs_low
	ldr r0, =XIP_QMI_BASE
        ldr r3, =QMI_DIRECT_TX_NOPUSH
        orrs tos, r3
	str tos, [r0, #QMI_DIRECT_TX_OFFSET]
	pull_tos
	lsrs r1, tos, 16
	movs r2, #0xFF
	ands r1, r2
        orrs r1, r3
	str r1, [r0, #QMI_DIRECT_TX_OFFSET]
	lsrs r1, tos, 8
	ands r1, r2
        orrs r1, r3
	str r1, [r0, #QMI_DIRECT_TX_OFFSET]
	ands tos, r2
        orrs tos, r3
	str tos, [r0, #QMI_DIRECT_TX_OFFSET]
	pull_tos
	pop {pc}
	end_inlined

	@ Erase flash ( addr cmd -- )
	define_word "erase-flash", visible_flag
_erase_flash:
	push {lr}

@        push_tos
@        movs tos, 0x0D
@        bl _emit
@        push_tos
@        movs tos, 0x0A
@        bl _emit
@        push_tos
@        bl _h_2
@        push_tos
@        movs tos, 0x20
@        bl _emit
@        push_tos
@        ldr tos, [r7, #4]
@        bl _h_8
        
	bl _enable_flash_cmd
	bl _enable_flash_write
	bl _write_flash_address
	bl _force_flash_cs_high
	bl _wait_flash_write_busy

	pop {pc}
	end_inlined

.ltorg

	@ Enable writing
	define_word "enable-flash-write", visible_flag
_enable_flash_write:
	push {lr}
	bl _force_flash_cs_low
	ldr r0, =XIP_QMI_BASE
	ldr r1, =CMD_WRITE_ENABLE | QMI_DIRECT_TX_NOPUSH
	str r1, [r0, #QMI_DIRECT_TX_OFFSET]
	bl _wait_qmi_busy
	bl _force_flash_cs_high
	pop {pc}
	end_inlined
	
	@ Read a status register
	define_word "read-flash-status", visible_flag
_read_flash_status:
	push {lr}
	bl _force_flash_cs_low
	ldr r0, =XIP_QMI_BASE
	str tos, [r0, #QMI_DIRECT_TX_OFFSET]
	str tos, [r0, #QMI_DIRECT_TX_OFFSET]
	bl _wait_qmi_busy
	ldr r0, =XIP_QMI_BASE
	ldr tos, [r0, #QMI_DIRECT_RX_OFFSET]
	ldr tos, [r0, #QMI_DIRECT_RX_OFFSET]
	bl _force_flash_cs_high
	pop {pc}
	end_inlined

	@ Wait for write busy to finish
	define_word "wait-flash-write-busy" visible_flag
_wait_flash_write_busy:
	push {lr}
1:	bl _force_flash_cs_low
	ldr r0, =XIP_QMI_BASE
	movs r1, #CMD_READ_STATUS
	str r1, [r0, #QMI_DIRECT_TX_OFFSET]
	str r1, [r0, #QMI_DIRECT_TX_OFFSET]
	bl _wait_qmi_busy
	ldr r0, =XIP_QMI_BASE
	ldr r1, [r0, #QMI_DIRECT_RX_OFFSET]
	ldr r1, [r0, #QMI_DIRECT_RX_OFFSET]
	push {r1}
	bl _force_flash_cs_high
	pop {r1}
	movs r2, #WRITE_BUSY_STATE
	tst r1, r2
	bne 1b
	pop {pc}
	end_inlined

	@ Enable QSPI
	define_word "enable-flash-qspi", visible_flag
_enable_flash_qspi:	
	push {lr}
	@ Enable writing
	bl _enable_flash_write

	@ Enable QSPI
	bl _force_flash_cs_low
	ldr r1, =CMD_WRITE_STATUS | QMI_DIRECT_TX_NOPUSH
	ldr r0, =XIP_QMI_BASE
	str r1, [r0, #QMI_DIRECT_TX_OFFSET]
	ldr r3, =0 | QMI_DIRECT_TX_NOPUSH
	ldr r2, =QSPI_ENABLE_STATE | QMI_DIRECT_TX_NOPUSH
	str r3, [r0, #QMI_DIRECT_TX_OFFSET]
	str r2, [r0, #QMI_DIRECT_TX_OFFSET]
	bl _wait_qmi_busy
	bl _force_flash_cs_high

	@ Wait for writing to finish
	bl _wait_flash_write_busy
	
	pop {pc}
	end_inlined

	@ Enter XIP
	define_word "enter-xip", visible_flag
_enter_xip:
	push {lr}
	
	@ Enable sending commands
	bl _enable_flash_cmd

	@ Read the status register
	push_tos
	movs tos, #CMD_READ_STATUS_2
	bl _read_flash_status
	
	@ Check for whether QSPI is enabled and skip initialization if so
	movs r2, #QSPI_ENABLE_STATE
	movs r1, tos
	pull_tos
	cmp r1, r2
	beq 1f

	@ Enable QSPI
	bl _enable_flash_qspi
	
	@ Actually set up XIP
1:	bl _force_flash_cs_normal

        @ Disable direct mode
        ldr r0, =XIP_QMI_BASE
        ldr r1, =QMI_DIRECT_CSR_EN
        ldr r2, [r0, #QMI_DIRECT_CSR_OFFSET]
        bics r2, r1
        str r2, [r0, #QMI_DIRECT_CSR_OFFSET]

	@ Prime XIP
        ldr r1, =XIP_QMI_BASE

        ldr r2, =INIT_M0_TIMING
        str r2, [r1, #QMI_M0_TIMING_OFFSET]
        dmb
        dsb
        isb

        ldr r2, =INIT_XIP_QMI_M0_RFMT
        str r2, [r1, #QMI_M0_RFMT_OFFSET]
        ldr r2, =INIT_XIP_QMI_M0_RCMD
        str r2, [r1, #QMI_M0_RCMD_OFFSET]

        dmb
        dsb
        isb

        ldr r0, =flash_start
        ldr r0, [r0]

        dmb
        dsb
        isb
        
	@ Set up continuing XIP automatically
        ldr r1, =XIP_QMI_BASE
        ldr r2, =CONT_XIP_QMI_M0_RFMT
        str r2, [r1, #QMI_M0_RFMT_OFFSET]
        ldr r2, =CONT_XIP_QMI_M0_RCMD
        str r2, [r1, #QMI_M0_RCMD_OFFSET]
        
	pop {pc}
	end_inlined

.ltorg
	
	@ Disable the XIP cache
	define_word "disable-xip-cache", visible_flag
_disable_xip_cache:
	ldr r0, =XIP_CTRL_BASE
	movs r1, XIP_CTRL_EN_NONSECURE | XIP_CTRL_EN_SECURE
	ldr r2, [r0, #XIP_CTRL_OFFSET]
	bics r2, r1
	str r2, [r0, #XIP_CTRL_OFFSET]
	bx lr
	end_inlined

	@ Enable and flush the XIP cahe
	define_word "enable-flush-xip-cache", visible_flag
_enable_flush_xip_cache:
	push {lr}
        push_tos
        ldr tos, ='F | ('C << 8)
        bl _call_rom_0
	ldr r0, =XIP_CTRL_BASE
	ldr r2, [r0, #XIP_CTRL_OFFSET]
        movs r1, XIP_CTRL_EN_NONSECURE | XIP_CTRL_EN_SECURE
	orrs r2, r1
	str r2, [r0, #XIP_CTRL_OFFSET]
	bl _force_flash_cs_normal
	pop {pc}
	end_inlined

	@ Wait for busy to clear
	define_word "wait-qmi-busy", visible_flag
_wait_qmi_busy:
	ldr r0, =XIP_QMI_BASE
	movs r1, #QMI_DIRECT_CSR_BUSY
1:      ldr r3, [r0, #QMI_DIRECT_CSR_OFFSET]
	tst r3, r1
	bne 1b
	bx lr
	end_inlined

	@ Call a ROM routine with zero parameters
	define_word "call-rom-0", visible_flag
_call_rom_0:
	push {lr}
        movs r2, #0
	movs r1, #RT_FLAG_FUNC_ARM_SEC
	movs r0, tos
	pull_tos
	ldrh r3, [r2, #0x16]
	blx r3
	blx r0
        pop {pc}
	end_inlined
	
	@ Exception handler for flash writes where flash has already been
	@ written
	define_word "x-flash-already-written", visible_flag
_store_flash_already_written:
	push {lr}
	string_ln "flash already written"
	bl _type
	pop {pc}
	end_inlined
	
	@ Exception handler for flash writes where flash has already been
	@ written
        define_word "x-write-core-flash", visible_flag
_attempted_to_write_core_flash:
	push {lr}
	string_ln "attempted to write to core flash"
	bl _type
	pop {pc}
	end_inlined

	@ Exception handler for flash writes past the end of flash
        define_word "x-write-past-flash-end", visible_flag
_attempted_to_write_past_flash_end:
	push {lr}
	string_ln "attempted to write past flash end"
	bl _type
	pop {pc}
	end_inlined

	@ Enter serial-mode and then exit XIP - flushing the cache to clear
	@ forcing the CS pin is needed before turning to XIP ( -- )
	define_internal_word "exit-xip", visible_flag
_exit_xip:
	push {lr}
	push_tos
	ldr tos, ='E | ('X << 8)
	bl _call_rom_0
	pop {pc}
	end_inlined

	@ Erase a QSPI 64K sector
	define_internal_word "erase-qspi-sector", visible_flag
_erase_qspi_sector:
	push {lr}
	bl _force_core_wait
	cpsid i
	dsb
	isb
	bl _exit_xip
	ldr r0, =flash_start
	subs tos, r0
	ldr r0, =0xFFFF
	bics tos, r0
	push_tos
	ldr tos, =CMD_BLOCK_ERASE_64K
	bl _erase_flash
	bl _enable_flush_xip_cache
	bl _enter_xip
	cpsie i
        bl _release_core
	pop {pc}
	end_inlined

	@ Erase a QSPI 4K sector
	define_internal_word "erase-qspi-4k-sector", visible_flag
_erase_qspi_4k_sector:
	push {lr}
	bl _force_core_wait
	cpsid i
	dsb
	isb
	bl _exit_xip
	ldr r0, =flash_start
	subs tos, r0
	ldr r0, =0xFFF
	bics tos, r0
	push_tos
	ldr tos, =CMD_SECTOR_ERASE
	bl _erase_flash
	bl _enable_flush_xip_cache
	bl _enter_xip
	cpsie i
        bl _release_core
	pop {pc}
	end_inlined

	@ Erase after a given address (including the sector the address is in)
	define_internal_word "erase-range", visible_flag
_erase_range:
	push {lr}
	bl _force_core_wait
	cpsid i
	dsb
	isb
	bl _exit_xip
	ldr r2, =0xFFF
        movs r0, tos
        pull_tos
	movs r1, tos
	pull_tos
        bics r0, r2
	bics r1, r2
1:	cmp r1, r0
	bhs 2f
	ldr r2, =0xFFFF
	tst r1, r2
	bne 3f
        ldr r2, =flash_dict_main_end - flash_start
        cmp r1, r2
        bhs 3f
	push_tos
	movs tos, r1
	push_tos
	ldr tos, =CMD_BLOCK_ERASE_64K
	push {r0, r1}
	bl _erase_flash
	pop {r0, r1}
	ldr r2, =0x10000
	adds r1, r2
	b 1b
3:	ldr r2, =0x7FFF
	tst r1, r2
	bne 4f
	push_tos
	movs tos, r1
	push_tos
	ldr tos, =CMD_BLOCK_ERASE_32K
	push {r0, r1}
	bl _erase_flash
	pop {r0, r1}
	ldr r2, =0x8000
	adds r1, r2
	b 1b
4:	push_tos
	movs tos, r1
	push_tos
	ldr tos, =CMD_SECTOR_ERASE
	push {r0, r1}
	bl _erase_flash
	pop {r0, r1}
	ldr r2, =0x1000
	adds r1, r2
	b 1b
2:	bl _enable_flush_xip_cache
	bl _enter_xip
@	cpsie i
	pop {pc}
	end_inlined

.ltorg
	
	@ Erase after a given address (including the sector the address is in)
	@ and reboot.
	define_internal_word "erase-after", visible_flag
_erase_after:
	push {lr}
	ldr r0, =flash_start
	subs tos, r0
        push_tos
        ldr tos, =flash_main_end - flash_start
	bl _erase_range
        ldr r0, =reboot_hook
        ldr r1, =_do_nothing
        str r1, [r0]
	bl _reboot
	pop {pc}
	end_inlined

	@ Erase the dictionary after a given address (including the sector the
        @ address is in and reboot.
	define_internal_word "erase-dict-after", visible_flag
_erase_dict_after:
	push {lr}
	ldr r0, =flash_start
	subs tos, r0
        push_tos
        ldr tos, =flash_main_end - flash_start
	bl _erase_range
        ldr r0, =reboot_hook
        ldr r1, =_do_nothing
        str r1, [r0]
	bl _reboot
	pop {pc}
	end_inlined

	@ Erase all flash except for the zeptoforth runtime
	define_word "erase-all", visible_flag
_erase_all:
	push {lr}
	push_tos
	ldr tos, =flash_min_address
	bl _erase_after
	pop {pc}
	end_inlined
		
	@ Find the end of the flash dictionary
	define_internal_word "find-flash-end", visible_flag
_find_flash_end:
	push_tos
	ldr tos, =flash_dict_end
	ldr r1, =flash_dict_start
1:	cmp tos, r1
	beq 2f
	subs tos, #4
	ldr r0, [tos]
	adds r0, #1
	bne 2f
	subs tos, #4
	ldr r0, [tos]
	adds r0, #1
	bne 2f
	subs tos, #4
	ldr r0, [tos]
	adds r0, #1
	bne 2f
	subs tos, #4
	ldr r0, [tos]
	adds r0, #1
	beq 1b
2:	bx lr
	end_inlined

	@ Find the next flash block
	define_internal_word "next-flash-block", visible_flag
_next_flash_block:
	movs r0, #0xFF
	bics tos, r0
	ldr r0, =256
	adds tos, r0
	bx lr
	end_inlined

	@ Find the start of the last flash word
	define_internal_word "find-last-flash-word", visible_flag
_find_last_flash_word:
	ldr r1, =0xDEADBEEF
	ldr r2, =flash_dict_start
1:	subs tos, #4
	cmp tos, r2
	beq 2f
	ldr r0, [tos]
	cmp r0, r1
	bne 1b
3:	subs tos, #4
	cmp tos, r2
	beq 2f
	ldr r0, [tos]
	cmp r0, r1
	beq 3b
	ldr tos, [tos]
	bx lr
2:	ldr tos, =last_kernel_word
	bx lr
	end_inlined

	@ Initiate writing to flash
	define_word "init-flash-write", visible_flag
_init_flash_write:
	push {lr}
	cpsid i
	bl _force_core_wait
	bl _exit_xip
	bl _enable_flash_cmd
	bl _enable_flash_write
	ldr r1, =flash_start
	subs tos, r1
	push_tos
	ldr tos, =CMD_PAGE_PROGRAM
	bl _write_flash_address
        bl _wait_qmi_busy

        
@        @ Debugging LED display
@        ldr r0, =SIO_BASE
@        ldr r1, =1 << 25
@        str r1, [r0, #GPIO_OE_SET]
@        str r1, [r0, #GPIO_OUT_SET]


	pop {pc}
	end_inlined

	@ Wait until the TX FIFO is empty
_wait_tx_empty:
	ldr r0, =XIP_QMI_BASE + QMI_DIRECT_CSR_OFFSET
	ldr r2, =QMI_DIRECT_CSR_TXEMPTY
1:	ldr r3, [r0]
	tst r3, r2
	beq 1b
	bx lr
	end_inlined

        @ Empty the QMI RX FIFO
_empty_qmi_rx_fifo:
	ldr r0, =XIP_QMI_BASE
	ldr r2, =QMI_DIRECT_CSR_RXEMPTY
1:	ldr r3, [r0, #QMI_DIRECT_CSR_OFFSET]
	tst r3, r2
	bne 2f
        ldr r1, [r0, #QMI_DIRECT_RX_OFFSET]
        b 1b
2:      bx lr
	end_inlined

	@ Actually mass write a buffer to QSPI flash ( data-addr bytes -- )
	define_internal_word "actually-mass-qspi!", visible_flag
_actually_store_mass_qspi:
	push {lr}
	movs r1, tos
	pull_tos
1:	cmp r1, #0
	beq 2f
	subs r1, #1
	ldrb r2, [tos]
	adds tos, #1
	ldr r0, =XIP_QMI_BASE
        ldr r3, =QMI_DIRECT_TX_NOPUSH
        orrs r2, r3
	str r2, [r0, #QMI_DIRECT_TX_OFFSET]
	push {r1}
	bl _wait_qmi_busy
	pop {r1}
	b 1b
2:	bl _force_flash_cs_high
	bl _wait_flash_write_busy
	pull_tos
	pop {pc}
	end_inlined
	
	@ Write a buffer to QSPI flash ( data-addr bytes addr -- )
	define_internal_word "mass-qspi!", visible_flag
_store_mass_qspi:
	push {lr}
	ldr r0, =flash_dict_start
	cmp tos, r0
	blo 1f
	cpsid i
	bl _force_core_wait
	bl _exit_xip
	bl _enable_flash_cmd
	movs r0, tos
	pull_tos
	ldr r1, =flash_start
	subs r0, r1
	movs r1, tos
	pull_tos
	movs r2, tos
	pull_tos
2:	cmp r1, #0
	beq 3f
	push {r0, r1, r2}
	bl _enable_flash_write
	pop {r0, r1, r2}
	push_tos
	movs tos, r0
	push_tos
	ldr tos, =CMD_PAGE_PROGRAM
	push {r0, r1, r2}
	bl _write_flash_address
	bl _wait_qmi_busy
	pop {r0, r1, r2}
	push {r4}
	ldr r3, =QSPI_PAGE_SIZE - 1
	movs r4, r0
	subs r4, #1
	orrs r3, r4
	adds r3, #1
	subs r3, r0
	pop {r4}
	cmp r3, #0
	bne 5f
	ldr r3, =0x100
5:	cmp r1, r3
	bhi 4f
	movs r3, r1
4:	push_tos
	movs tos, r2
	push_tos
	movs tos, r3
	push {r0, r1, r2, r3}
	bl _actually_store_mass_qspi
	pop {r0, r1, r2, r3}
	subs r1, r3
	adds r0, r3
	adds r2, r3
	b 2b
3:	bl _enable_flush_xip_cache
	bl _enter_xip
	cpsie i
	bl _release_core
	pop {pc}
1:	ldr tos, =_attempted_to_write_core_flash
	bl _raise
	bx lr
	end_inlined

.ltorg
	
	@ Write a byte to QSPI flash
	define_internal_word "cqspi!", visible_flag
_store_qspi_1:	
	push {lr}
	ldrb r0, [tos]
	movs r1, 0xFF
	cmp r0, r1
	bne 1f
	ldr r0, =flash_dict_start
	cmp tos, r0
	blo 3f
	bl _init_flash_write
	ldr r0, =XIP_QMI_BASE
	movs r1, #0xFF
	ands tos, r1
        ldr r3, =QMI_DIRECT_TX_NOPUSH
        orrs tos, r3
	str tos, [r0, #QMI_DIRECT_TX_OFFSET]
	pull_tos
	bl _wait_qmi_busy
	bl _force_flash_cs_high
	bl _wait_flash_write_busy
	bl _enable_flush_xip_cache
	bl _enter_xip
	cpsie i
	bl _release_core
	pop {pc}
1:	ldr tos, =_store_flash_already_written
	bl _raise
3:	ldr tos, =_attempted_to_write_core_flash
	bl _raise
	pop {pc}

	@ Write a halfword to QSPI flash
	define_internal_word "hqspi!", visible_flag
_store_qspi_2:
	push {lr}
	movs r1, #0xFF
	movs r2, tos
	ands r2, r1
	cmp r2, r1
	beq 1f
	ldrh r0, [tos]
	ldr r1, =0xFFFF
	cmp r0, r1
	bne 2f
	ldr r0, =flash_dict_start
	cmp tos, r0
	blo 4f
	bl _init_flash_write
	ldr r0, =XIP_QMI_BASE
	movs r1, #0xFF
	movs r2, tos
	ands r2, r1
        ldr r3, =QMI_DIRECT_TX_NOPUSH
        orrs r2, r3
	str r2, [r0, #QMI_DIRECT_TX_OFFSET]
	lsrs r2, tos, #8
	ands r2, r1
        orrs r2, r3
	str r2, [r0, #QMI_DIRECT_TX_OFFSET]
	pull_tos
	bl _wait_qmi_busy
	bl _force_flash_cs_high
	bl _wait_flash_write_busy
	bl _enable_flush_xip_cache
	bl _enter_xip
	cpsie i
	bl _release_core
	pop {pc}
1:	ldr r1, [dp]
	movs r3, tos
	movs r2, #0xFF
	movs r0, r1
	ands r0, r2
	str r0, [dp]
	push {r1, r2, r3}
	bl _store_qspi_1
	pop {r1, r2, r3}
	push_tos
	lsrs tos, r1, #8
	ands tos, r2
	push_tos
	movs tos, r3
	adds r3, #1
	bl _store_qspi_1
	pop {pc}
2:	ldr tos, =_store_flash_already_written
	bl _raise
4:	ldr tos, =_attempted_to_write_core_flash
	bl _raise
	pop {pc}
	end_inlined
	
	@ Write a word to QSPI flash
	define_internal_word "qspi!", visible_flag
_store_qspi_4:
	push {lr}
	movs r1, #0xFF
	movs r2, tos
	ands r2, r1
	movs r1, #0xFC
	cmp r2, r1
	bhi 1f
	ldr r0, [tos]
	ldr r1, =0xFFFFFFFF
	cmp r0, r1
	bne 2f
	ldr r0, =flash_dict_start
	cmp tos, r0
	blo 4f
	bl _init_flash_write
	ldr r0, =XIP_QMI_BASE
	movs r1, #0xFF
	movs r2, tos
	ands r2, r1
        ldr r3, =QMI_DIRECT_TX_NOPUSH
        orrs r2, r3
	str r2, [r0, #QMI_DIRECT_TX_OFFSET]
	lsrs r2, tos, #8
	ands r2, r1
        orrs r2, r3
	str r2, [r0, #QMI_DIRECT_TX_OFFSET]
	lsrs r2, tos, #16
	ands r2, r1
        orrs r2, r3
	str r2, [r0, #QMI_DIRECT_TX_OFFSET]
	lsrs r2, tos, #24
        orrs r2, r3
	str r2, [r0, #QMI_DIRECT_TX_OFFSET]
	pull_tos
	bl _wait_qmi_busy
	bl _force_flash_cs_high
	bl _wait_flash_write_busy
	bl _enable_flush_xip_cache
	bl _enter_xip
	cpsie i
	bl _release_core
	pop {pc}
1:	ldr r1, [dp]
	movs r3, tos
	ldr r2, =0xFFFF
	movs r0, r1
	ands r0, r2
	str r0, [dp]
	push {r1, r2, r3}
	bl _store_qspi_2
	pop {r1, r2, r3}
	push_tos
	lsrs tos, r1, #16
	ands tos, r2
	push_tos
	movs tos, r3
	adds r3, #1
	bl _store_qspi_2
	pop {pc}
2:	ldr tos, =_store_flash_already_written
	bl _raise
4:	ldr tos, =_attempted_to_write_core_flash
	bl _raise
	pop {pc}
	end_inlined

	@ Write two words to QSPI flash
	define_internal_word "2qspi!", visible_flag
_store_qspi_8:
	push {lr}
	push {tos}
	bl _store_qspi_4
	push_tos
	pop {tos}
	adds tos, #4
	bl _store_qspi_4
	pop {pc}
	end_inlined

	@ Write a byte to an address in a flash buffer
	define_word "cflash!", visible_flag
_store_flash_1:
	ldr r0, =flash_end - 1
	cmp tos, r0
	bhi 2f
	b _store_qspi_1
2:	ldr tos, =_attempted_to_write_past_flash_end
	bl _raise
	bx lr
	end_inlined
	
	@ Write a halfword to an address in one or more flash_buffers
	define_word "hflash!", visible_flag
_store_flash_2:
	ldr r0, =flash_end - 2
	cmp tos, r0
	bhi 3f
	b _store_qspi_2
3:	ldr tos, =_attempted_to_write_past_flash_end
	bl _raise
	bx lr
	end_inlined

	@ Write a word to an address in one or more flash_buffers
	define_word "flash!", visible_flag
_store_flash_4:
	ldr r0, =flash_end - 4
	cmp tos, r0
	bhi 3f
	b _store_qspi_4
3:	ldr tos, =_attempted_to_write_past_flash_end
	bl _raise
	bx lr
	end_inlined

	@ Write a word to an address in one or more flash_buffers
	define_word "2flash!", visible_flag
_store_flash_8:
	push {lr}
	push {tos}
	bl _store_flash_4
	push_tos
	pop {tos}
	adds tos, #4
	bl _store_flash_4
	pop {pc}
	end_inlined

	@ Flush all the buffered flash
	define_internal_word "flush-all-flash", visible_flag
_flush_all_flash:	
	bx lr
	end_inlined

	@ Fill flash until it is aligned to a block
	define_word "flash-block-align,", visible_flag
_flash_block_align:
	bx lr
	end_inlined

	@ Get the flash block size in bytes
	define_word "flash-block-size", visible_flag | inlined_flag
_flash_block_size:
	push_tos
	ldr tos, =flash_block_size
	bx lr
	end_inlined

	@ Initialize the flash buffers
	define_internal_word "init-flash-buffers", visible_flag
_init_flash_buffers:
	bx lr
	end_inlined

        @ Placeholder for cflash-buffer@ on other architectures, same as c@
        define_internal_word "cflash-buffer@", visible_flag
_get_flash_buffer_value_1:
        ldrb tos, [tos]
        bx lr
        end_inlined
        
        @ Placeholder for hflash-buffer@ on other architectures, same as h@
        define_internal_word "hflash-buffer@", visible_flag
_get_flash_buffer_value_2:
        ldrh tos, [tos]
        bx lr
        end_inlined

        @ Placeholder for flash-buffer@ on other architectures, same as @
        define_internal_word "flash-buffer@", visible_flag
_get_flash_buffer_value_4:
        ldr tos, [tos]
        bx lr
        end_inlined

        @ Get the Pico unique ID
	define_word "unique-id", visible_flag
_unique_id:     
	push_tos
	ldr r0, =pico_uuid
	ldr tos, [r0, #4]
	push_tos
	ldr tos, [r0, #8]
	bx lr
	end_inlined

	.ltorg
