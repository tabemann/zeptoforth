@ Copyright (c) 2013 Matthias Koch
@ Copyright (c) 2019-2022 Travis Bemann
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

	.equ MAX_ERASE_ADDR, 0x00100000
	.equ FLASH_CODA_ADDR, 0x100FFF00
	
	.equ XIP_CTRL_BASE, 0x14000000
	.equ XIP_CTRL_OFFSET, 0x00
	.equ XIP_FLUSH_OFFSET, 0x04
	.equ XIP_STAT_OFFSET, 0x08

	.equ XIP_SSI_BASE, 0x18000000

	.equ RAM_BASE, 0x20000000
	.equ FLASH_IMAGE_BASE, 0x10001000
	.equ IMAGE_SIZE, 0x8000
	.equ IO_QSPI_BASE, 0x40018000
	.equ PADS_QSPI_BASE, 0x40020000
	.equ XIP_SSI_BASE, 0x18000000
	.equ SSI_CTRLR0_OFFSET, 0x00
	.equ SSI_CTRLR1_OFFSET, 0x04
	.equ SSI_SSIENR_OFFSET, 0x08
	.equ SSI_BAUDR_OFFSET, 0x14
	.equ SSI_SR_OFFSET, 0x28
	.equ SSI_DR0_OFFSET, 0x60 @ 36 words
	.equ SSI_SPI_CTRLR0_OFFSET, 0xF4

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

	.equ IO_QSPI_GPIO_QSPI_SS_CTRL_OFFSET, 0x0C
	.equ IO_QSPI_GPIO_QSPI_SS_CTRL_OUTOVER_MASK, 0x300
	.equ IO_QSPI_GPIO_QSPI_SS_CTRL_OUTOVER_NORMAL, 0x000
	.equ IO_QSPI_GPIO_QSPI_SS_CTRL_OUTOVER_INVERT, 0x100
	.equ IO_QSPI_GPIO_QSPI_SS_CTRL_OUTOVER_LOW, 0x200
	.equ IO_QSPI_GPIO_QSPI_SS_CTRL_OUTOVER_HIGH, 0x300

	@ QSPI page size
	.equ QSPI_PAGE_SIZE, 256
	
	@ Mode to send commands
	@ Standard SPI mode (0 << 21)
	@ 8 clocks per data frame (7 << 16)
	@ Tx and Rx (0 << 8)
	.equ CTRLR0_SEND_CMD, ((0 << 21) | (7 << 16) | (0 << 8))
	
	@ Quad SPI mode (2 << 21)
	@ 32 clocks per data frame (31 << 16)
	@ Send instruction and address, receive data (3 << 8)
	.equ CTRLR0_INIT_XIP, ((2 << 21) | (31 << 16) | (3 << 8))

	@ 4 wait cycles (WAIT_CYCLES << 11)
	@ 8 bit command prefix (2 << 8)
	@ 32 address and mode bits (8 << 2)
	@ Command is SPI, address is QSPI (1 << 0)
	.equ SPI_CTRLR0_INIT_XIP, ((WAIT_CYCLES << 11) | (2 << 8) | (8 << 2) | (1 << 0))

	@ Continually send reads
	@ Send CMD_CONT_READ (CMD_CONT_READ << 24)
	@ 4 wait cycles (WAIT_CYCLES << 11)
	@ 0 bit command prefix (0 << 8)
	@ 32 address and mode bits (8 << 2)
	@ Command is QSPI, address is QSPI (2 << 0)
	.equ SPI_CTRLR0_CONT_XIP, ((CMD_CONT_READ << 24) | (WAIT_CYCLES << 11) | (0 << 8) | (8 << 2) | (2 << 0))

	@ 0 wait cycles (0 << 11)
	@ 8 bit command prefix (2 << 8)
	@ 24 address and mode bits (6 << 2)
	@ Command and address are SPI (0 << 0)
	.equ SPI_CTRLR0_ERASE, ((0 << 11) | (2 << 8) | (6 << 2) | (0 << 0))

	@ Erase flash up to a specified address if needed
	define_internal_word "init-flash", visible_flag
_init_flash:
	push {lr}
	ldr r0, =FLASH_CODA_ADDR
	ldr r1, =0xFFFFFFFF
	ldr r0, [r0]
	cmp r0, r1
	beq 1f
	push_tos
	movs tos, r0
	bl _erase_range
	cpsie i
	bl _release_core
1:	pop {pc}
	end_inlined

	@ Force the CS pin HIGH
	define_word "force-flash-cs-high", visible_flag
_force_flash_cs_high:
	push {lr}
	push_tos
	ldr tos, =IO_QSPI_GPIO_QSPI_SS_CTRL_OUTOVER_HIGH
	bl _force_flash_cs
	pop {pc}
	end_inlined

	@ Force the CS pin LOW
	define_word "force-flash-cs-low", visible_flag
_force_flash_cs_low:
	push {lr}
	push_tos
	ldr tos, =IO_QSPI_GPIO_QSPI_SS_CTRL_OUTOVER_LOW
	bl _force_flash_cs
	pop {pc}
	end_inlined

	@ Force the CS pin NORMAL
	define_word "force-flash-cs-normal", visible_flag
_force_flash_cs_normal:
	push {lr}
	push_tos
	ldr tos, =IO_QSPI_GPIO_QSPI_SS_CTRL_OUTOVER_NORMAL
	bl _force_flash_cs
	pop {pc}
	end_inlined
	
	@ Force the CS pin
	define_word "force-flash-cs", visible_flag
_force_flash_cs:
	ldr r0, =IO_QSPI_BASE + IO_QSPI_GPIO_QSPI_SS_CTRL_OFFSET
	ldr r1, [r0]
	ldr r2, =IO_QSPI_GPIO_QSPI_SS_CTRL_OUTOVER_MASK
	bics r1, r2
	orrs r1, tos
	str r1, [r0]
	ldr r1, [r0]
	pull_tos
	bx lr
	end_inlined
	
	@ Enable sending commands
	define_word "enable-flash-cmd", visible_flag
_enable_flash_cmd:
	push {lr}
	bl _disable_ssi
	ldr r0, =XIP_SSI_BASE + SSI_CTRLR0_OFFSET
	ldr r1, =CTRLR0_SEND_CMD
	str r1, [r0]
	bl _enable_ssi
	pop {pc}
	end_inlined

	@ Write an address to flash ( addr cmd -- )
	define_word "write-flash-address", visible_flag
_write_flash_address:
	push {lr}
	bl _force_flash_cs_low
	ldr r0, =XIP_SSI_BASE
	str tos, [r0, #SSI_DR0_OFFSET]
	pull_tos
	lsrs r1, tos, 16
	movs r2, #0xFF
	ands r1, r2
	str r1, [r0, #SSI_DR0_OFFSET]
	lsrs r1, tos, 8
	ands r1, r2
	str r1, [r0, #SSI_DR0_OFFSET]
	ands tos, r2
	str tos, [r0, #SSI_DR0_OFFSET]
	pull_tos
	pop {pc}
	end_inlined

	@ Erase flash ( addr cmd -- )
	define_word "erase-flash", visible_flag
_erase_flash:
	push {lr}
	bl _enable_flash_cmd
	bl _enable_flash_write
	bl _write_flash_address
	bl _wait_ssi_busy
	ldr r0, =XIP_SSI_BASE
	ldr r1, [r0, #SSI_DR0_OFFSET]
	ldr r1, [r0, #SSI_DR0_OFFSET]
	ldr r1, [r0, #SSI_DR0_OFFSET]
	ldr r1, [r0, #SSI_DR0_OFFSET]
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
	ldr r0, =XIP_SSI_BASE
	movs r1, #CMD_WRITE_ENABLE
	str r1, [r0, #SSI_DR0_OFFSET]
	bl _wait_ssi_busy
	ldr r0, =XIP_SSI_BASE
	ldr r1, [r0, #SSI_DR0_OFFSET]
	bl _force_flash_cs_high
	pop {pc}
	end_inlined
	
	@ Read a status register
	define_word "read-flash-status", visible_flag
_read_flash_status:
	push {lr}
	bl _force_flash_cs_low
	ldr r0, =XIP_SSI_BASE
	str tos, [r0, #SSI_DR0_OFFSET]
	str tos, [r0, #SSI_DR0_OFFSET]
	bl _wait_ssi_busy
	ldr r0, =XIP_SSI_BASE
	ldr tos, [r0, #SSI_DR0_OFFSET]
	ldr tos, [r0, #SSI_DR0_OFFSET]
	bl _force_flash_cs_high
	pop {pc}
	end_inlined

	@ Wait for write busy to finish
	define_word "wait-flash-write-busy" visible_flag
_wait_flash_write_busy:
	push {lr}
1:	bl _force_flash_cs_low
	ldr r0, =XIP_SSI_BASE
	movs r1, #CMD_READ_STATUS
	str r1, [r0, #SSI_DR0_OFFSET]
	str r1, [r0, #SSI_DR0_OFFSET]
	bl _wait_ssi_busy
	ldr r0, =XIP_SSI_BASE
	ldr r1, [r0, #SSI_DR0_OFFSET]
	ldr r1, [r0, #SSI_DR0_OFFSET]
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
	movs r1, #CMD_WRITE_STATUS
	ldr r0, =XIP_SSI_BASE
	str r1, [r0, #SSI_DR0_OFFSET]
	movs r3, #0
	movs r2, #QSPI_ENABLE_STATE
	str r3, [r0, #SSI_DR0_OFFSET]
	str r2, [r0, #SSI_DR0_OFFSET]
	bl _wait_ssi_busy
	ldr r0, =XIP_SSI_BASE
	ldr r3, [r0, #SSI_DR0_OFFSET]
	ldr r3, [r0, #SSI_DR0_OFFSET]
	ldr r3, [r0, #SSI_DR0_OFFSET]
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

	@ First disable the SSI
	bl _disable_ssi
	
	@ Set up the registers
	ldr r0, =XIP_SSI_BASE
	ldr r1, =CTRLR0_INIT_XIP
	str r1, [r0, #SSI_CTRLR0_OFFSET]
	movs r1, #0
	str r1, [r0, #SSI_CTRLR1_OFFSET]
	ldr r1, =SPI_CTRLR0_INIT_XIP
	ldr r2, =XIP_SSI_BASE + SSI_SPI_CTRLR0_OFFSET
	str r1, [r2]

	@ Enable SSI
	bl _enable_ssi

	@ Prime XIP
	ldr r0, =XIP_SSI_BASE
	movs r1, #CMD_READ_DATA_FAST_QUAD_IO
	str r1, [r0, #SSI_DR0_OFFSET]
	movs r1, #CMD_CONT_READ
	str r1, [r0, #SSI_DR0_OFFSET]
	bl _wait_ssi_busy

	@ Disable SSI
	bl _disable_ssi

	@ Set up continuing XIP automatically
	ldr r1, =SPI_CTRLR0_CONT_XIP
	ldr r2, =XIP_SSI_BASE + SSI_SPI_CTRLR0_OFFSET
	str r1, [r2]

	@ Enable SSI
	bl _enable_ssi

	pop {pc}
	end_inlined

.ltorg
	
	@ Enable SSI
	define_word "enable-ssi", visible_flag
_enable_ssi:
	ldr r0, =XIP_SSI_BASE
	movs r1, #1
	str r1, [r0, #SSI_SSIENR_OFFSET]
	bx lr
	end_inlined

	@ Disable SSI
	define_word "disable-ssi", visible_flag
_disable_ssi:
	ldr r0, =XIP_SSI_BASE
	movs r1, #0
	str r1, [r0, #SSI_SSIENR_OFFSET]
	bx lr
	end_inlined

	@ Disable the XIP cache
	define_word "disable-xip-cache", visible_flag
_disable_xip_cache:
	ldr r0, =XIP_CTRL_BASE
	movs r1, 0x1
	ldr r2, [r0, #XIP_CTRL_OFFSET]
	bics r2, r1
	str r2, [r0, #XIP_CTRL_OFFSET]
	bx lr
	end_inlined

	@ Enable and flush the XIP cahe
	define_word "enable-flush-xip-cache", visible_flag
_enable_flush_xip_cache:
	push {lr}
	ldr r0, =XIP_CTRL_BASE
	movs r1, 0x1
	str r1, [r0, #XIP_FLUSH_OFFSET]
	ldr r2, [r0, #XIP_FLUSH_OFFSET] @ Wait until flushing completes
	ldr r2, [r0, #XIP_CTRL_OFFSET]
	orrs r2, r1
	str r2, [r0, #XIP_CTRL_OFFSET]
	bl _force_flash_cs_normal
	pop {pc}
	end_inlined

	@ Wait for busy to clear
	define_word "wait-ssi-busy", visible_flag
_wait_ssi_busy:
	ldr r0, =XIP_SSI_BASE + SSI_SR_OFFSET
	movs r1, #1 @ BUSY
	movs r2, #4 @ TX FIFO empty
1:	ldr r3, [r0]
	tst r3, r2
	beq 1b
	tst r3, r1
	bne 1b
	bx lr
	end_inlined

	@ Call a ROM routine with zero parameters
	define_word "call-rom-0", visible_flag
_call_rom_0:
	push {lr}
	movs r2, #0
	movs r1, tos
	pull_tos
	ldrh r0, [r2, #0x14]
	ldrh r3, [r2, #0x18]
	blx r3
	blx r0
	pop {pc}
	end_inlined
	
	@ Exception handler for flash writes where flash has already been
	@ written
	define_word "flash-already-written", visible_flag
_store_flash_already_written:
	push {lr}
	string_ln "flash already written"
	bl _type
	pop {pc}
	end_inlined
	
	@ Exception handler for flash writes where flash has already been
	@ written
_attempted_to_write_core_flash:
	push {lr}
	string_ln "attempted to write to core flash"
	bl _type
	pop {pc}
	end_inlined

	@ Exception handler for flash writes past the end of flash
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
	pop {pc}
	end_inlined

	@ Erase after a given address (including the sector the address is in)
	define_internal_word "erase-range", visible_flag
_erase_range:
	push {lr}
	bl _force_core_wait
	cpsid i
	bl _exit_xip
	ldr r0, =0xFFF
	movs r1, tos
	pull_tos
	bics r1, r0
1:	ldr r0, =MAX_ERASE_ADDR
	cmp r1, r0
	bhs 2f
	ldr r0, =0xFFFF
	tst r1, r0
	bne 3f
	push_tos
	movs tos, r1
	push_tos
	ldr tos, =CMD_BLOCK_ERASE_64K
	push {r1}
	bl _erase_flash
	pop {r1}
	ldr r0, =0x10000
	adds r1, r0
	b 1b
3:	ldr r0, =0x7FFF
	tst r1, r0
	bne 4f
	push_tos
	movs tos, r1
	push_tos
	ldr tos, =CMD_BLOCK_ERASE_32K
	push {r1}
	bl _erase_flash
	pop {r1}
	ldr r0, =0x8000
	adds r1, r0
	b 1b
4:	push_tos
	movs tos, r1
	push_tos
	ldr tos, =CMD_SECTOR_ERASE
	push {r1}
	bl _erase_flash
	pop {r1}
	ldr r0, =0x1000
	adds r1, r0
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
	bl _erase_range
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
	pop {pc}
	end_inlined

	@ Wait until the TX FIFO is empty
_wait_tx_empty:
	ldr r0, =XIP_SSI_BASE + SSI_SR_OFFSET
	movs r2, #4 @ TX FIFO empty
1:	ldr r3, [r0]
	tst r3, r2
	beq 1b
	bx lr
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
	ldr r0, =XIP_SSI_BASE
	str r2, [r0, #SSI_DR0_OFFSET]
	push {r1}
	bl _wait_ssi_busy
	pop {r1}
	ldr r0, =XIP_SSI_BASE
	ldr r2, [r0, #SSI_DR0_OFFSET]
	b 1b
2:	ldr r0, =XIP_SSI_BASE
	ldr r2, [r0, #SSI_DR0_OFFSET]
	ldr r2, [r0, #SSI_DR0_OFFSET]
	ldr r2, [r0, #SSI_DR0_OFFSET]
	ldr r2, [r0, #SSI_DR0_OFFSET]
	bl _force_flash_cs_high
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
	ldr r0, =XIP_SSI_BASE
	movs r1, #0xFF
	ands tos, r1
	str tos, [r0, #SSI_DR0_OFFSET]
	pull_tos
	bl _wait_ssi_busy
	ldr r0, =XIP_SSI_BASE
	ldr r1, [r0, #SSI_DR0_OFFSET]
	ldr r1, [r0, #SSI_DR0_OFFSET]
	ldr r1, [r0, #SSI_DR0_OFFSET]
	ldr r1, [r0, #SSI_DR0_OFFSET]
	ldr r1, [r0, #SSI_DR0_OFFSET]
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
	ldr r0, =XIP_SSI_BASE
	movs r1, #0xFF
	movs r2, tos
	ands r2, r1
	str r2, [r0, #SSI_DR0_OFFSET]
	lsrs r2, tos, #8
	ands r2, r1
	str r2, [r0, #SSI_DR0_OFFSET]
	pull_tos
	bl _wait_ssi_busy
	ldr r0, =XIP_SSI_BASE
	ldr r1, [r0, #SSI_DR0_OFFSET]
	ldr r1, [r0, #SSI_DR0_OFFSET]
	ldr r1, [r0, #SSI_DR0_OFFSET]
	ldr r1, [r0, #SSI_DR0_OFFSET]
	ldr r1, [r0, #SSI_DR0_OFFSET]
	ldr r1, [r0, #SSI_DR0_OFFSET]	
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
	ldr r0, =XIP_SSI_BASE
	movs r1, #0xFF
	movs r2, tos
	ands r2, r1
	str r2, [r0, #SSI_DR0_OFFSET]
	lsrs r2, tos, #8
	ands r2, r1
	str r2, [r0, #SSI_DR0_OFFSET]
	lsrs r2, tos, #16
	ands r2, r1
	str r2, [r0, #SSI_DR0_OFFSET]
	lsrs r2, tos, #24
	str r2, [r0, #SSI_DR0_OFFSET]
	pull_tos
	bl _wait_ssi_busy
	ldr r0, =XIP_SSI_BASE
	ldr r1, [r0, #SSI_DR0_OFFSET]
	ldr r1, [r0, #SSI_DR0_OFFSET]
	ldr r1, [r0, #SSI_DR0_OFFSET]
	ldr r1, [r0, #SSI_DR0_OFFSET]
	ldr r1, [r0, #SSI_DR0_OFFSET]
	ldr r1, [r0, #SSI_DR0_OFFSET]	
	ldr r1, [r0, #SSI_DR0_OFFSET]
	ldr r1, [r0, #SSI_DR0_OFFSET]	
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

	.ltorg
	
