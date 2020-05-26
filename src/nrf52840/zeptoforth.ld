MEMORY
{
	flash : org = 0x00027000, len = 836k
	ram : org = 0x20000008, len = 0x3FFF8
}

SECTIONS
{
	.text : { *(.text*) } > flash
	.bss : { *(.bss*) } > ram
}
