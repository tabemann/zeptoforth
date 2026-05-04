# Bus Control Words

The RP2040 and RP2350 support controlling bus priorities of CPU 0, CPU 1, DMA reads, and DMA writes. These bus priorities consist of a binary of either low priority or high priority. The words below enable setting and getting these priorities

### `busctrl`

The `busctrl` module contains the following words:

##### `high`
( -- priority )

High bus priority

##### `low`
( -- priority )

Low bus priority

##### `dma-w-bus-priority!`
( priority -- )

Set DMA write bus priority

##### `dma-w-bus-priority@`
( -- priority )

Get DMA write bus priority

##### `dma-r-bus-priority!`
( priority -- )

Set DMA read bus priority

##### `dma-r-bus-priority@`
( -- priority )

Get DMA read bus priority

##### `proc1-bus-priority!`
( priority -- )

Set CPU 1 bus priority

##### `proc1-bus-priority@`
( -- priority )

Get CPU 1 bus priority
  
##### `proc0-bus-priority!`
( priority -- )

Set CPU 0 bus priority

##### `proc0-bus-priority@`
( -- priority )

Get CPU 0 bus priority
