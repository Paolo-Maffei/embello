\ Test SRAM access via FSMC, the setup code is now in ../sram.fs

forgetram

\ see http://compgroups.net/comp.lang.forth/random-number-generator/1259025
\ only last bit should be used, but for this purpose we can use all 32 bits

0 variable seed

: random ( -- u ) seed @ dup ror or seed @ rol xor dup seed ! ;

: r 0 do random hex. loop ;

: sram-test ( u -- )  \ test first N bytes of SRAM, original data is lost
\ sram-init
  $12345678 seed !
  dup 0 do
    random SRAM i + !  \ fill it with random values
  4 +loop
\ SRAM $10 dump
  $12345678 seed !
  0 do
    random SRAM i + @  \ now read those values back and compare
    <> if i . ." FAILED!" quit then
  4 +loop ;

: sram-full ( -- )  \ test entire 1 MB SRAM, then clear its contents
  20 bit  dup sram-test  SRAM swap 0 fill ;

\ : sram-time ( -- )  \ measure read and write times for a full scan
\   sram-init
\   micros  20 bit 0 do                      4 +loop  micros swap - .
\   micros  20 bit 0 do  i             drop  4 +loop  micros swap - .
\   micros  20 bit 0 do    SRAM i + @ drop  4 +loop  micros swap - .
\   micros  20 bit 0 do  i SRAM i + !       4 +loop  micros swap - . ;

123 .
1000 sram-test
456 .
sram-full
789 .

: a micros sram-full micros swap - . ." us " ;
: b 0 do sram-full [char] . emit loop ;
a
cr 1000 b
