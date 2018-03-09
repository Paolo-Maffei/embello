\ core definitions

\ <<<board>>>
compiletoflash
( core start: ) here hex.

PB10 constant LED

\ ZIF socket pin allocations

PC0  constant Z01
PA5  constant Z02  \ VPP DAC
PC2  constant Z03
PC4  constant Z04
PC3  constant Z05
PC5  constant Z06
PB11 constant Z07  \ VCC
PB8  constant Z08
PB9  constant Z09
PC1  constant Z10  \ GND
PA0  constant Z11
PA2  constant Z12  \ GND
PA1  constant Z13
PA3  constant Z14
PD0  constant Z15
PA8  constant Z16
PC11 constant Z17
PC9  constant Z18
PC7  constant Z19
PB15 constant Z20
PB13 constant Z21  \ GND
PD1  constant Z22
PC12 constant Z23
PC10 constant Z24
PC8  constant Z25
PC6  constant Z26
PB14 constant Z27
PB12 constant Z28  \ VCC

PA4  constant Z02-VPP  \ ADC
PB4  constant Z07-VCC
PB0  constant Z10-GND
PB1  constant Z12-GND
PB5  constant Z21-GND
PD2  constant Z28-VCC

include ../flib/mecrisp/quotation.fs
include ../flib/mecrisp/multi.fs
include ../flib/any/timed.fs

cornerstone <<<core>>>
hello
