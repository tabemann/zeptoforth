MEMORY
{
	flash : org = 0x00026000, len = 328k
	ram : org = 0x20000000, len = 0x10000
}

SECTIONS
{
	.text : { *(.text*) } > flash
	.bss : { *(.bss*) } > ram
}
