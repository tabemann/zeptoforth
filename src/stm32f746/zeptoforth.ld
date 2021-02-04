MEMORY
{
	flash : org = 0x00200000, len = 1024k
	ram : org = 0x20000000, len = 320k
}

SECTIONS
{
	.text : { *(.text*) } > flash
	.bss : { *(.bss*) } > ram
}
