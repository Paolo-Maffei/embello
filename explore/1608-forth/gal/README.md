This is an STM32F103RC board, intended for use as GAL programmer.

Pinout should make it possible to support 16v8, 20v8, 22v10, and 26v12.  
Two 2x14 headers, with all 51 I/O pins, as well as 5V/3.3V/GND.

                  J2                              J3
             ------------                    ------------
           9  pb9 ^^ pb8   8            mosi  -  pa7 -- pa6   - miso
           7 pb11    pb10  - (led)       dac  -  pa4 ~? pa5   - e-sense
    15-28 21 pb13    pb12 28 15-28       osc  - pc15 -- pc14  - osc
    15-28 20 pb15    pb14 27 15-28          !28  pd2    pa15  - a-nss
     1-14 10  pc1 -- pc0   1 1-14        i2c  -  pb6    pb7   - i2c
     1-14  5  pc3 -- pc2   3 1-14            !7  pb4    pb3   - a-sck
     1-14  6  pc5 -- pc4   4 1-14       1-14 ..  pa1 -- pa0  .. 1-14
    15-28 19  pc7    pc6  26 15-28      1-14 ..  pa3 -- pa2  .. 1-14
    15-28 18  pc9    pc8  25 15-28       swd    pa14    pa13  - swd
    15-28 17 pc11    pc10 24 15-28          !21  pb5 -- pb0  10!
    15-28 16  pa8 ^  pc12 23 15-28       usb  - pa12    pa11  - usb
    15-28 15  pd0 x  pb2   - (boot)       rx  - pa10    pa9   - tx
           -   nc  x pd1  22 15-28      vppe  - pc13 -- pb1  12!
           -  gnd    3v3   -                  -  gnd    5v    -
           \              /                   \               /
            +--- ZIF ---+                      +---- ZIF ----+

^ = pwm-able, x = rtc xtal (disabled), - = not 5V-tolerant,
! = mosfet control, ~ = dac, ? = adc

Pin allocation leaves free pins for 1x UART, 1x SPI, 1x I2C, and SWD.

ZIF socket connections:

     1       pc0            28 vcc  pb12  pd2!
     2 vpp   pa4~ pa5?      27      pb14 
     3       pc2            26      pc6  
     4       pc4            25      pc8  
     5       pc3            24      pc10 
     6       pc5            23      pc12 
     7 vcc   pb11 pb4!      22      pd1  
     8       pb8            21 gnd  pb13  pb5!
     9       pb9            20      pb15 
    10 gnd   pc1  pb0!      19      pc7  
    11       1-14           18      pc9  
    12 gnd   1-14 pb1!      17      pc11 
    13       1-14           16      pa8  
    14       1-14           15      pd0  

gnd = n-mosfet (3x), vcc = p-mosfet w/ pull-up to 5V on gate (2x)  
vpp = dac w/ power opa548 in 6x mode, vppe = opa548 output enable  
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

Note: pin 1 is always in the top left, regardless of chip pin count.
