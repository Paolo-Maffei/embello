// Convert the df32.mif file to Forth source, for loading into flash memory.

#include <stdint.h>
#include <stdio.h>

uint16_t mem [32768];

int main () {
    FILE* fp = fopen("df32.mif", "r");
    if (fp == 0) {
	perror("df32.mif");
	return 1;
    }

    char line [100];
    while (fgets(line, sizeof line, fp) != 0) {
	int addr, data;
	if (sscanf(line, "%o:%o;", &addr, &data) == 2)
	    mem[addr] = data;
    }

/*
    $00010000 variable maddr
    : f^ ( -- )  \ erase flash before filling it
         maddr @ dup $10000 + swap
         do
	   [char] . emit i flashpageerase
	 1024 +loop
    ;
    : f, ( a u -- a )  \ save word to flash and advance address
      2dup swap hflash!
      16 rshift over 2+ hflash!
      4 +
    ;
    : m, ( u1 u2 u3 u4 u5 u6 u7 u8 -- )  \ save next 32 bytes to flash
      >r >r >r >r >r >r >r >r
      maddr @
      r> f, r> f, r> f, r> f, r> f, r> f, r> f, r> f,
      maddr !
    ;
*/

    printf("$00010000 variable maddr\n");
    printf(": f^ maddr @ dup $10000 + swap\n");
    printf("     do [char] . emit i flashpageerase 1024 +loop ;\n");
    printf(": f, 2dup swap hflash! 16 rshift over 2+ hflash! 4 + ;\n");
    printf(": m, >r >r >r >r >r >r >r >r maddr @\n");
    printf("     r> f, r> f, r> f, r> f, r> f, r> f, r> f, r> f, maddr ! ;\n");
    printf("f^ hex\n");
    for (int i = 0; i < sizeof mem / 4; i += 8) {
	printf(" ");
	for (int j = 0; j < 8; ++j)
	    printf(" %08x", ((const uint32_t*) mem)[i+j]);
	printf(" m,\n");
    }
    printf("decimal\n");

    return 0;
}
