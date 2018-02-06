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
  IMODE-ADC PA4 io-mode!   \ set pin to analog mode
  1 DAC-CR !               \ enable DAC
;

: dac-dma ( addr count -- )  \ feed DAC from wave table at given address
  0 bit RCC-AHBENR bis!  \ DMAEN clock enable
     $90 DMA-CSELR bis!  \ remap DMA ch.2 to DAC

          2/ DMA-CNDTR2 !  \ 2-byte entries
              DMA-CMAR2 !  \ read from address passed as input
  DAC-DHR12R1 DMA-CPAR2 !  \ write to DAC

                0   \ register settings for CCR2 of DMA:
  %01 10 lshift or  \ MSIZE = 16-bits
   %01 8 lshift or  \ PSIZE = 16 bits
          7 bit or  \ MINC
          5 bit or  \ CIRC
          4 bit or  \ DIR = from mem to peripheral
          0 bit or  \ EN
       DMA-CCR2 !

  \ set up DAC to convert on each write from DMA
  12 bit DAC-CR bis!  \ DMAEN1
;

: dac-awg ( u -- )  \ generate on DAC1 via DMA with given timer period in us
  clock-hz @ 500000 */ 1-  \ convert ms cycle time to per-sample clock ticks
  6 timer-init  dac-init  SINE500 1000 dac-dma ;

20 dac-awg  \ 20 ms = 50 Hz sine wave
