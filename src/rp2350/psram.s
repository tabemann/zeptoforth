@ Copyright (c) 2024 Travis Bemann
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

        @ QMI CS1n function
        .equ QMI_CS1N_FUNCTION, 9
        
        @ For PSRAM timing calculations - to use int math, we work in
        @ femtoseconds (fs) (1e-15),
        @ NOTE: This idea is from micro python work on psram..

        @ Number of nanoseconds in a second
        .equ SEC_TO_NS, 1000000000

        @ max select pulse width = 8us => 8e6 ns => 8000 ns => 8000 * 1e6 fs =>
        @ 8000e6 fs
        @ Additionally, the MAX select is in units of 64 clock cycles - will
        @ use a constant that  takes this into account - so 8000e6 fs / 64 =
        @ 125e6 fs
        .equ PSRAM_MAX_SELECT_FS64, 125000000

        @ min deselect pulse width = 50ns => 50 * 1e6 fs => 50e7 fs
        .equ PSRAM_MIN_DESELECT_FS, 50000000

        @ from psram datasheet - max Freq with VDDat 3.3v - SparkFun RP2350
        @ boards run at 3.3v.
        @ If VDD = 3.0 Max Freq is 133 Mhz
        .equ PSRAM_MAX_SCK_HZ, 109000000
        
        .equ PSRAM_CMD_QUAD_END, 0xF5
        .equ PSRAM_CMD_QUAD_ENABLE, 0x35
        .equ PSRAM_CMD_READ_ID, 0x9F
        .equ PSRAM_CMD_RSTEN, 0x66
        .equ PSRAM_CMD_RST, 0x99
        .equ PSRAM_CMD_QUAD_READ, 0xEB
        .equ PSRAM_CMD_QUAD_WRITE, 0x38
        .equ PSRAM_CMD_NOOP, 0xFF

        .equ PSRAM_ID, 0x5D

        .equ PSRAM_CMD_QUAD_END_FULL, QMI_DIRECT_TX_OE | QMI_DIRECT_TX_IWIDTH_QUAD | PSRAM_CMD_QUAD_END

        .equ PSRAM_M1_TIMING_PART, QMI_M1_TIMING_PAGEBREAK_1024_BYTE | (3 << QMI_M1_TIMING_SELECT_HOLD_LSB) | (1 << QMI_M1_TIMING_COOLDOWN_LSB) | (1 << QMI_M1_TIMING_RXDELAY_LSB)

        .equ PADS_BANK0_GPIO_DRIVE_LSB, 4
        .equ PADS_BANK0_GPIO_SLEWFAST_BITS, 1 << 0
        
        .equ INIT_PAD_QMI_CS1N, (2 << PADS_BANK0_GPIO_DRIVE_LSB) | PADS_BANK0_GPIO_SLEWFAST_BITS

        .equ DIRECT_CSR_CSN1, QMI_DIRECT_CSR_ASSERT_CS1N | QMI_DIRECT_CSR_EN

        .equ INIT_QMI_M1_RFMT, (QUAD_WIDTH << QMI_M1_RFMT_PREFIX_WIDTH_LSB) | (QUAD_WIDTH << QMI_M1_RFMT_ADDR_WIDTH_LSB) | (QUAD_WIDTH << QMI_M1_RFMT_SUFFIX_WIDTH_LSB) | (QUAD_WIDTH << QMI_M1_RFMT_DUMMY_WIDTH_LSB) | QMI_M1_RFMT_DUMMY_LEN_24_BITS | (QUAD_WIDTH << QMI_M1_RFMT_DATA_WIDTH_LSB) | QMI_M1_RFMT_PREFIX_LEN_8_BITS

        .equ INIT_QMI_M1_WFMT, (QUAD_WIDTH << QMI_M1_WFMT_PREFIX_WIDTH_LSB) | (QUAD_WIDTH << QMI_M1_WFMT_ADDR_WIDTH_LSB) | (QUAD_WIDTH << QMI_M1_WFMT_SUFFIX_WIDTH_LSB) | (QUAD_WIDTH << QMI_M1_WFMT_DUMMY_WIDTH_LSB) | QMI_M1_WFMT_DUMMY_LEN_0_BITS | (QUAD_WIDTH << QMI_M1_WFMT_DATA_WIDTH_LSB) | QMI_M1_WFMT_PREFIX_LEN_8_BITS

        .equ INIT_QMI_M1_RCMD, (PSRAM_CMD_QUAD_READ << QMI_M1_RCMD_PREFIX_LSB) | (0 << QMI_M1_RCMD_SUFFIX_LSB)

        .equ INIT_QMI_M1_WCMD, (PSRAM_CMD_QUAD_WRITE << QMI_M1_WCMD_PREFIX_LSB) | (0 << QMI_M1_WCMD_SUFFIX_LSB)

        @ Disable direct CSR
        define_internal_word "disable-cs1n-direct-csr", visible_flag
_disable_cs1n_direct_csr:
        ldr r0, =XIP_QMI_BASE
        ldr r1, =DIRECT_CSR_CSN1
        ldr r2, [r0, #QMI_DIRECT_CSR_OFFSET]
        bics r2, r1
        str r2, [r0, #QMI_DIRECT_CSR_OFFSET]
        bx lr
        end_inlined
        
        @ Get the PSRAM size in bytes ( -- psram-bytes )
        define_internal_word "get-psram-size", visible_flag
_get_psram_size:
        push {r4, lr}

        bl _enable_flash_cmd
        bl _wait_qmi_busy
        bl _empty_qmi_rx_fifo

        ldr r0, =XIP_QMI_BASE
        ldr r1, [r0, #QMI_DIRECT_CSR_OFFSET]
        ldr r2, =QMI_DIRECT_CSR_ASSERT_CS1N
        orrs r1, r2
        str r1, [r0, #QMI_DIRECT_CSR_OFFSET]

        ldr r2, =PSRAM_CMD_QUAD_END_FULL
        str r2, [r0, #QMI_DIRECT_TX_OFFSET]

        bl _wait_qmi_busy

        ldr r0, =XIP_QMI_BASE
        ldr r2, [r0, #QMI_DIRECT_RX_OFFSET]
        
        ldr r1, [r0, #QMI_DIRECT_CSR_OFFSET]
        ldr r2, =QMI_DIRECT_CSR_ASSERT_CS1N
        bics r1, r2
        str r1, [r0, #QMI_DIRECT_CSR_OFFSET]

        ldr r1, [r0, #QMI_DIRECT_CSR_OFFSET]
        orrs r1, r2
        str r1, [r0, #QMI_DIRECT_CSR_OFFSET]

        movs r2, #0 @ index
        movs r3, #0 @ kgd
        movs r4, #0 @ eid

1:      cmp r2, #0
        bne 2f
        ldr r1, =PSRAM_CMD_READ_ID
        b 3f
2:      ldr r1, =PSRAM_CMD_NOOP
3:      str r1, [r0, #QMI_DIRECT_TX_OFFSET]
        push {r0, r2, r3}
        bl _wait_tx_empty
        bl _wait_qmi_busy
        pop {r0, r2, r3}
        cmp r2, #5
        bne 4f
        ldr r3, [r0, #QMI_DIRECT_RX_OFFSET]
        b 6f
4:      cmp r2, #6
        bne 5f
        ldr r4, [r0, #QMI_DIRECT_RX_OFFSET]
        b 6f
5:      ldr r1, [r0, #QMI_DIRECT_RX_OFFSET]
6:      adds r2, #1
        cmp r2, #7
        blo 1b

        ldr r1, [r0, #QMI_DIRECT_CSR_OFFSET]
        ldr r2, =QMI_DIRECT_CSR_ASSERT_CS1N
        bics r1, r2
        str r1, [r0, #QMI_DIRECT_CSR_OFFSET]

        push_tos
        movs tos, #0
        cmp r3, #PSRAM_ID
        bne 3f
        ldr tos, =1024 * 1024
        lsls r0, r4, #5
        cmp r4, #0x26
        bne 2f
        cmp r0, #2
        bne 2f
        ldr tos, =8 * 1024 * 1024
        b 3f
2:      cmp r0, #0
        bne 2f
        ldr tos, =2 * 1024 * 1024
        b 3f
2:      cmp r0, #1
        bne 3f
        ldr tos, =4 * 1024 * 1024
        
3:      bl _disable_cs1n_direct_csr

        pop {r4, pc}
        end_inlined
        
        @ Update the PSRAM timing configuration based on the system clock
        define_internal_word "set-psram-timing", visible_flag
_set_psram_timing:
        push {lr}

        ldr r0, =sysclk
        ldr r0, [r0]

        ldr r1, =PSRAM_MAX_SCK_HZ
        movs r2, r0
        adds r2, r1
        subs r2, #1
        udiv r1, r2, r1
        lsls r1, r1, #QMI_M1_TIMING_CLKDIV_LSB @ r1: clock divider field
        
        ldr r2, =SEC_TO_NS
        udiv r0, r2, r0
        ldr r2, =1000000
        muls r0, r0, r2 @ r0: femtoseconds per cycle

        ldr r2, =PSRAM_MAX_SELECT_FS64
        udiv r2, r2, r0
        lsls r2, r2, #QMI_M1_TIMING_MAX_SELECT_LSB @ r2: max select field
        orrs r1, r2

        ldr r2, =PSRAM_MIN_DESELECT_FS
        adds r2, r0
        subs r2, #1
        udiv r2, r2, r0
        lsls r2, r2, #QMI_M1_TIMING_MIN_DESELECT_LSB @ r2: min deselect field
        orrs r1, r2

        ldr r2, =PSRAM_M1_TIMING_PART
        orrs r1, r2

        ldr r0, =XIP_QMI_BASE
        str r1, [r0, #QMI_M1_TIMING_OFFSET]

        pop {pc}
        end_inlined

        @ Initialize PSRAM ( psram-cs-pin -- )
        define_word "init-psram", visible_flag
_init_psram:
        push {lr}

        cmp tos, #0
        beq 1f
        cmp tos, #8
        beq 1f
        cmp tos, #19
        beq 1f
        cmp tos, #47
        beq 1f
        ldr tos, =_x_invalid_qmi_cs1n_pin
        bl _raise

1:      lsls r0, tos, #3
        ldr r1, =IO_BANK0_BASE + 4
        adds r0, r1

        ldr r1, [r0]
        movs r2, #0x1F
        bics r1, r2
        movs r2, #QMI_CS1N_FUNCTION
        orrs r1, r2
        str r1, [r0]

        lsls r0, tos, #2
        ldr r1, =PADS_BANK0_BASE + 4
        adds r0, r1

        ldr r1, =INIT_PAD_QMI_CS1N
        str r1, [r0]

        pull_tos

        cpsid i
        bl _force_core_wait
        
        bl _get_psram_size
        cmp tos, #0
        bne 1f
        cpsie i
        bl _release_core
        push_tos
        ldr tos, =_x_no_psram
        bl _raise
        pop {pc}

1:      bl _enable_flash_cmd
        bl _wait_qmi_busy

        ldr r0, =XIP_QMI_BASE
        movs r1, #0
1:      ldr r2, [r0, #QMI_DIRECT_CSR_OFFSET]
        ldr r3, =QMI_DIRECT_CSR_ASSERT_CS1N
        orrs r2, r3
        str r2, [r0, #QMI_DIRECT_CSR_OFFSET]

        cmp r1, #0
        bne 2f
        ldr r3, =PSRAM_CMD_RSTEN
        b 3f
2:      cmp r1, #1
        bne 2f
        ldr r3, =PSRAM_CMD_RST
        b 3f
2:      ldr r3, =PSRAM_CMD_QUAD_ENABLE
3:      str r3, [r0, #QMI_DIRECT_TX_OFFSET]

        push {r0, r1}
        bl _wait_qmi_busy
        pop {r0, r1}

        ldr r2, [r0, #QMI_DIRECT_CSR_OFFSET]
        ldr r3, =QMI_DIRECT_CSR_ASSERT_CS1N
        bics r2, r3
        str r2, [r0, #QMI_DIRECT_CSR_OFFSET]
        
        nop
        nop
        nop
        nop
        nop

        nop
        nop
        nop
        nop
        nop

        nop
        nop
        nop
        nop
        nop

        nop
        nop
        nop
        nop
        nop

        ldr r2, [r0, #QMI_DIRECT_RX_OFFSET]

        adds r1, #1
        cmp r1, #3
        blo 1b
        
        bl _disable_cs1n_direct_csr

        bl _set_psram_timing
        
        bl _disable_xip_cache
        
        ldr r0, =XIP_QMI_BASE
        ldr r1, =INIT_QMI_M1_RFMT
        str r1, [r0, #QMI_M1_RFMT_OFFSET]
        ldr r1, =INIT_QMI_M1_RCMD
        str r1, [r0, #QMI_M1_RCMD_OFFSET]
        ldr r1, =INIT_QMI_M1_WFMT
        str r1, [r0, #QMI_M1_WFMT_OFFSET]
        ldr r1, =INIT_QMI_M1_WCMD
        str r1, [r0, #QMI_M1_WCMD_OFFSET]

        ldr r0, =XIP_CTRL_BASE
        ldr r1, [r0, #XIP_CTRL_OFFSET]
        ldr r2, =XIP_CTRL_WRITABLE_M1
        orrs r1, r2
        str r1, [r0, #XIP_CTRL_OFFSET]
        
        bl _enable_flush_xip_cache
        
        cpsie i
        bl _release_core

        ldr r0, =psram_size
        str tos, [r0]

        pull_tos

        pop {pc}
        end_inlined

        define_word "x-no-psram", visible_flag
_x_no_psram:
        push {lr}
        string_ln "no PSRAM available"
        bl _type
        pop {pc}
        end_inlined

        define_word "x-invalid-qmi-cs1n-pin", visible_flag
_x_invalid_qmi_cs1n_pin:
        push {lr}
        string_ln "invalid QMI CS1n pin"
        bl _type
        pop {pc}
        end_inlined
        
