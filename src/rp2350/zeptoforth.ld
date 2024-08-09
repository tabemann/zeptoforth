MEMORY
{
	ram : org = 0x20000000, len = 264k
}

SECTIONS
{
	. = ORIGIN(ram);
	.text : { *(.text*) } > ram
	.bss : { *(.bss*) } > ram
}
