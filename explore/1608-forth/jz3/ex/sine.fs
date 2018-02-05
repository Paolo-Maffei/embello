\ Generate a sine wave via the DAC and DMA

forgetram

$40007400 constant DAC
    DAC $00 + constant DAC-CR
    DAC $08 + constant DAC-DHR12R1

$40020000 constant DMA
    DMA $1C + constant DMA-CCR2
    DMA $20 + constant DMA-CNDTR2
    DMA $24 + constant DMA-CPAR2
    DMA $28 + constant DMA-CMAR2
    DMA $A8 + constant DMA-CSELR

: dac! ( u -- )  \ send a new value to the DAC
  DAC-DHR12R1 ! ;

: dac-init ( -- )
  29 bit RCC-APB1ENR bis!  \ DACEN clock enable
  IMODE-ADC PA4 io-mode!
  1 DAC-CR !  \ enable
  0 dac! ;

: dac-dma ( addr count -- )  \ feed DAC from wave table at given address
  0 bit RCC-AHBENR bis!  \ DMAEN clock enable

          $90 DMA-CSELR bis!  \ remap DMA ch.2 to DAC
          2/ DMA-CNDTR2 !     \ 2-byte entries
              DMA-CMAR2 !     \ read from address passed as input
  DAC-DHR12R1 DMA-CPAR2 !     \ write to DAC

                0   \ register settings for CCR2 of DMA:
  %01 10 lshift or  \ MSIZE = 16-bits
   %01 8 lshift or  \ PSIZE = 16 bits
          7 bit or  \ MINC
          5 bit or  \ CIRC
          4 bit or  \ DIR = from mem to peripheral
          0 bit or  \ EN
       DMA-CCR2 !

  \ set up DAC to convert on each write from DMA1
  12 bit DAC-CR bis!  \ DMAEN1 and TEN1 and EN1
;

: dac-awg ( u -- )  \ generate on DAC1 via DMA with given timer period in us
  16 * \ clock frequency
  6 timer-init  dac-init  SINE1000 2000 dac-dma ;

: toggle
  begin
    1000 0 do
      i 2* SINE1000 + h@  dac!
      20 us
    loop
  key? until ;

\ dac-init
\ toggle
20 dac-awg
