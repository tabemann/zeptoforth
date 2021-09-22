MEMORY
{
	ram : org = 0x20041f00, len = 252
}

SECTIONS
{
	. = ORIGIN(ram);
	.text : { *(.text*) } > ram
}
