\ stop mode experiment
\ needs board.fs

cr cr reset
cr

\ include ../flib/stm32l0/sleep.fs

: highz-gpio
\ this s(h)aves another 0.6 µA ...
\ IMODE-ADC PA0  io-mode!
  IMODE-ADC PA1  io-mode!
  IMODE-ADC PA2  io-mode!
  IMODE-ADC PA3  io-mode!
\ IMODE-ADC PA4  io-mode!   \ SSEL
\ IMODE-ADC PA5  io-mode!   \ SCLK
\ IMODE-ADC PA6  io-mode!   \ MISO
\ IMODE-ADC PA7  io-mode!   \ MOSI
  IMODE-ADC PA8  io-mode!
  IMODE-ADC PA9  io-mode!
  IMODE-ADC PA10 io-mode!
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
  IMODE-ADC PB6  io-mode!
  IMODE-ADC PB7  io-mode!
  IMODE-ADC PC14 io-mode!
  IMODE-ADC PC15 io-mode!
;

: lp-blink ( -- )
  2.1MHz  1000 systick-hz only-msi
  OMODE-PP PA0 io-mode! \ for debugging
  PA0 ioc! 10 ms PA0 ios! 10 ms PA0 ioc!
  highz-gpio
  begin
    OMODE-PP PA0 io-mode! \ for debugging
    PA0 ios!
    led-on
    10 ms \ [IFDEF] rf-init  rf-recv drop  rf-sleep  [ELSE]  10 ms  [THEN]
    led-off
    PA0 ioc!
    stop1s
  again ;

[IFDEF] rf-init   ." rf-init" cr rf-init rf-sleep  [THEN]
[IFDEF] bme-init  ." bme-init" cr i2c-init bme-init drop  [THEN]
lptim-init lptim?

( clock-hz    ) clock-hz    @ .
( PWR-CR      ) PWR-CR      @ hex.
( PWR-CSR     ) PWR-CSR     @ hex.
( RCC-CR      ) RCC-CR      @ hex.
( RCC-CSR     ) RCC-CSR     @ hex.
( RCC-CCIPR   ) RCC-CCIPR   @ hex.
( RCC-AHBENR  ) RCC-AHBENR  @ hex.
( RCC-APB1ENR ) RCC-APB1ENR @ hex.
( RCC-APB2ENR ) RCC-APB2ENR @ hex.
( EXTI-IMR    ) EXTI-IMR    @ hex.
( EXTI-EMR    ) EXTI-EMR    @ hex.
( EXTI-PR     ) EXTI-PR     @ hex.
( SCR         ) SCR         @ hex.

led-off lp-blink
