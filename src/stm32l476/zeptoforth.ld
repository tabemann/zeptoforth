MEMORY
{
	flash : org = 0x00000000, len = 1024k
	ram : org = 0x20000000, len = 96k
}

SECTIONS
{
	.text : { *(.text*) } > flash
	.bss : { *(.bss*) } > ram
}
