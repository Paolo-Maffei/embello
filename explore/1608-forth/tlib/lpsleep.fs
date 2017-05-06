\ low power sleep

: highz-gpio
\ this s(h)aves another 0.6 µA ...
\ IMODE-ADC PA0  io-mode!   \ debug
\ IMODE-ADC PA1  io-mode!   \ debug
  IMODE-ADC PA2  io-mode!
  IMODE-ADC PA3  io-mode!
\ IMODE-ADC PA4  io-mode!   \ SSEL
\ IMODE-ADC PA5  io-mode!   \ SCLK
\ IMODE-ADC PA6  io-mode!   \ MISO
\ IMODE-ADC PA7  io-mode!   \ MOSI
  IMODE-ADC PA8  io-mode!
\ IMODE-ADC PA9  io-mode!   \ uart TX
\ IMODE-ADC PA10 io-mode!   \ uart RX
  IMODE-ADC PA11 io-mode!
  IMODE-ADC PA12 io-mode!
  IMODE-ADC PA13 io-mode!
  IMODE-ADC PA14 io-mode!
\ IMODE-ADC PA15 io-mode!   \ LED
  IMODE-ADC PB0  io-mode!
  IMODE-ADC PB1  io-mode!
  IMODE-ADC PB3  io-mode!
  IMODE-ADC PB4  io-mode!
  IMODE-ADC PB5  io-mode!
\ IMODE-ADC PB6  io-mode!   \ SCL
\ IMODE-ADC PB7  io-mode!   \ SDA
  IMODE-ADC PC14 io-mode!
  IMODE-ADC PC15 io-mode!
;

: low-power-sleep-simple ( n -- ) \ sleeps for n * 100ms
  rf-sleep highz-gpio
  -adc only-msi
  0 do stop100ms loop
  hsi-on adc-init ;

' low-power-sleep-simple variable *lps  \ pointer to low power sleep
: low-power-sleep *lps @ execute ;      \ indirect call via *lps
