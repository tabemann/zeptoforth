# NTC thermistor Support

There is an optional driver for thermistors of type NTC. 

An NTC thermistor is a thermal component that changes its resistance in response to temperature - higher the temperature, lower the resistance. This property can be utilized to measure temperature. However, the Raspberry Pi Pico can only measure voltage through its ADC input. To measure resistance, we need to use a voltage divider circuit:

```
  Pi Pico 3V3 -------+
   (3V3 - Pin36)     |
                     R0 - Resistor, R0 = RT at 25ºC
                     |
  Pi Pico ADC0 ------+
   (GP26 - Pin31)    |
                     RT - Thermistor
                     |
  Pi Pico GND -------+
   (GND - Pin28)
```

Since the resistance change is non-linear with temperature variation, the temperature can be calculated using two methods:
- Steinhart-Hart equation
- B-parameter approximation.

The driver determines which method to use for converting resistance to temperature based on the configured parameters.

NTC support is in `extra/rp_common/ntc.fs`, which can be compiled either to RAM or to flash as needed. There is a demo program which uses it in `test/rp_common/ntc_demo.fs`.

### `ntc`

The `ntc` module contains the following words:

##### `setup-adc`
( adc chan pin ntc -- )

Set up ADC.

##### `setup-abc` 
( D: a-val D: b-val D: c-val ntc -- )

Set up thermistor's parameters (values of a, b, c). These values are in S31.32 format

##### `setup-therm`
( D: vin D: r0 ntc -- )

Set up input voltage and resistance. These values are in S31.32 format.

##### `measure-ntc`
( ntc -- )  

Measure temperature.

##### `temp@`
( ntc -- D: kelvin D: celsius )

Put temperatures to stack. These values are in S31.32 format.

##### `dump-ntc`
( ntc -- )

Dump `ntc` structure.

##### `ntc-size`

The structure contains necessary data for temperature measurement (e.g., input voltage, resistance value, etc.). The measured voltage and the temperature calculated from it are also stored here.


