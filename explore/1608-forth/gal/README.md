This is an STM32F103RC board, intended for use as GAL programmer.

Pinout should make it possible to support 16v8, 20v8, 22v10, and 26v12.  
Two 2x14 headers, with all 51 I/O pins, as well as 5V/3.3V/GND.

                  J2                                 J3
             ------------                       ------------
           9  PB9 ^^ PB8   8            MOSI  -  PA7 -- PA6   - MISO
           7 PB11    PB10  - (LED)       DAC  -  PA4 ~? PA5   - VPP-IN
    15-28 21 PB13    PB12 28 15-28       OSC  - PC15 -- PC14  - OSC
    15-28 20 PB15    PB14 27 15-28          !28  PD2    PA15  - A-NSS
     1-14 10  PC1 -- PC0   1 1-14        I2C  -  PB6    PB7   - I2C
     1-14  5  PC3 -- PC2   3 1-14            !7  PB4    PB3   - A-SCK
     1-14  6  PC5 -- PC4   4 1-14       1-14 13  PA1 -- PA0  11 1-14
    15-28 19  PC7    PC6  26 15-28      1-14 14  PA3 -- PA2  12 1-14
    15-28 18  PC9    PC8  25 15-28       SWD    PA14    PA13  - SWD
    15-28 17 PC11    PC10 24 15-28          !21  PB5 -- PB0  10!
    15-28 16  PA8 ^  PC12 23 15-28       USB  - PA12    PA11  - USB
    15-28 15  PD0 X  PB2   - (BOOT)       RX  - PA10    PA9   - TX
           -   NC  X PD1  22 15-28      VPPE  - PC13 -- PB1  12!
           -  GND    3V3   -                  -  GND    5V    -
           \              /                   \               /
            +--- ZIF ---+                      +---- ZIF ----+

^ = pwm-able, x = rtc xtal (disabled), - = not 5V-tolerant,
! = mosfet control, ~ = dac, ? = adc

Pin allocation leaves free pins for 1x UART, 1x SPI, 1x I2C, USB, and SWD.

ZIF socket connections:

     1       PC0            28 VCC  PB12  PD2!
     2 VPP   PA5? PA4~      27      PB14 
     3       PC2            26      PC6  
     4       PC4            25      PC8  
     5       PC3            24      PC10 
     6       PC5            23      PC12 
     7 VCC   PB11 PB4!      22      PD1  
     8       PB8            21 GND  PB13  PB5!
     9       PB9            20      PB15 
    10 GND   PC1  PB0!      19      PC7  
    11       PA0            18      PC9  
    12 GND   PA2  PB1!      17      PC11 
    13       PA1            16      PA8  
    14       PA3            15      PD0  

gnd = n-mosfet (3x), vcc = p-mosfet w/ pull-up to 5V on gate (2x)  
vpp = dac & adc w/ power opa548 in 6x mode, vppe = opa548 output enable  
include jumper to switch vcc between 3.3V and 5V

GAL programming pinouts:

       16v8  20v8  22v10 26v12          26v12 22v10 20v8  16v8

     1  vil   vil   vil   vil        28  vil   vcc   vcc   vcc
     2  edit  edit  edit  edit       27  vil   vil   vil   p/v-
     3  ra1   ra1   p/v-  p/v-       26  vil   vil   p/v-  ra0
     4  ra2   ra2   ra0   ra0        25  vil   vil   ra0   vil
     5  ra3   ra3   ra1   ra1        24  vil   vil   vil   vil
     6  ra4   vil   ra2   ra2        23  vil   vil   vil   vil
     7  ra5   vil   ra3   vcc        22  vil   vil   vil   vil
     8  sclk  ra4   ra4   vil        21  gnd   vil   vil   vil
     9  sdin  ra5   ra5   ra3        20  vil   vil   vil   sdou
    10  gnd   sclk  sclk  ra4        19  vil   vil   sdou  stb-
    11        sdin  sdin  ra5        18  vil   sdou  vil
    12        gnd   gnd   sclk       17  vil   stb-  stb-
    13                    sdin       16  vil
    14                    stb-       15  sdou

Note: pin 1 is always in the top left, regardless of the chip's pin count.
