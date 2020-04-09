MEMORY
{
	flash : org = 0x00000000, len = 0x80000
	ram : org = 0x20000000, len = 0x10000
}

SECTIONS
{
	.text : { *(.text*) } > flash
	.bss : { *(.bss*) } > ram
}
