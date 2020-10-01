#include <MemoryFree.h>
#include <pgmStrToRAM.h>

#include <elapsedMillis.h>
#include <EEPROM.h>
#include <OneWire.h>
#include <DallasTemperature.h>
elapsedMillis pumponcommand;
elapsedMillis fanoncommand;
elapsedMillis switchcomptime;
elapsedMillis protection_timer;
// Data to Arduino pin 2
#define ONE_WIRE_BUS 2
#define pump 4
#define pump_signal 8
#define fan1 7
#define compressorB 6
#define compressorA 5
#define fan2 9
#define highPA A0
#define highPB A1
#define lowPA A2
#define lowPB A3
#define oilA A4
#define oilB A5

String incomingByte;
int set_temp = 0;
int donotenterhere = 0;
int pump_fail = 0;
int tmpCounter = 0;
char tmp[100];
int compressorAkey = 0;
int compressorBkey = 0;
int temp_is_set = 0;
int pumprelay = 0;
String sensor1;
String sensor2;
int negative = 0;
unsigned long switch_interval_for_eeprom = 0;
int flag = 0;
int pump_is_on = 0;
int i = 5;
int fan_is_on = 0;
int first_time = 0;
int init_comp = 0;
int currently_switching_compressor = 0;
int set_protection = 0;
int compressor_off_value_sent = 0;
int compressor_a_value_sent = 0;
int compressor_b_value_sent = 0;
int fan_1_value_sent = 0;
int fan_2_value_sent = 0;
unsigned long switch_interval = 0;
unsigned long freeze_comptime = 0;
int val;
int compressor_running = 0;
int z = 0;
int y = 0;
int maintain_breakout = 0;
int high_pressure_A_entry = 0;
int high_pressure_B_entry = 0;
int low_pressure_A_entry = 0;
int low_pressure_B_entry = 0;
int oil_pressure_A_entry = 0;
int oil_pressure_B_entry = 0;
OneWire oneWire(ONE_WIRE_BUS);

// Pass our oneWire reference to Dallas Temperature.
DallasTemperature sensors(&oneWire);

void setup(void)
{
  Serial.begin(115200);
  Serial.setTimeout(200);
  Serial.println("boot success");

  // Start up the library
  sensors.begin();
  sensors.setResolution(9);
  //delay(10000);
  pinMode(fan1, OUTPUT);        // connected to Relay 1
  pinMode(fan2, OUTPUT);        // connected to Relay 2
  pinMode(pump, OUTPUT);        // connected to Relay 3
  pinMode(compressorA, OUTPUT); // connected to Relay 4
  pinMode(compressorB, OUTPUT); // connected to Relay 5
  pinMode(pump_signal, INPUT_PULLUP);
  pinMode(highPA, INPUT_PULLUP);
  pinMode(highPB, INPUT_PULLUP);
  pinMode(lowPA, INPUT_PULLUP);
  pinMode(lowPB, INPUT_PULLUP);
  pinMode(oilA, INPUT_PULLUP);
  pinMode(oilB, INPUT_PULLUP);
  diagnostics();
  digitalWrite(fan1, LOW);        // connected to Relay 1
  digitalWrite(fan2, LOW);        // connected to Relay 2
  digitalWrite(pump, LOW);        // connected to Relay 3
  digitalWrite(compressorA, LOW); // connected to Relay 4
  digitalWrite(compressorB, LOW); // connected to Relay 5
  while (1)
  { //Serial.println("waiting for ready");
    if (Serial.available() > 0)
    {
      incomingByte = Serial.readString();
      Serial.println(incomingByte);
      if (incomingByte.startsWith("ready"))
      {
        if (EEPROM.read(2))
        {

          set_temp = EEPROM.read(0);
          negative = EEPROM.read(1);
          if (negative == 1)
          {
            set_temp = set_temp * -1;
          }
          Serial.print("setting");
          Serial.println(set_temp, DEC);
          temp_is_set = 1;
        }
        switch_interval_for_eeprom = EEPROM.read(5);
        switch_interval = switch_interval_for_eeprom * 60 * 1000;
        Serial.print("switch_interval ");
        Serial.println(switch_interval_for_eeprom, DEC);  //printing interval time in minutes
        if (switch_interval_for_eeprom <= 1)
        {
          switch_interval_for_eeprom = 1;
          switch_interval = switch_interval_for_eeprom * 60 * 1000;
        }
        //switch_interval = switch_interval_for_eeprom * 60 * 1000; //convert to milliseconds
        //Serial.println("program using milliseconds");
        Serial.println(switch_interval);
        break;
      }

    }
  }

  Serial.print(F("Free RAM = "));
  Serial.println(freeMemory(), DEC);
}

void loop(void)
{

  if (Serial.available() > 0)
  {
    incomingByte = Serial.readString();
    if (incomingByte.startsWith("manual"))
    {
      manualmode();
    }
    if (incomingByte.startsWith("auto"))
    {
      automode();
    }
  }
}

void automode()
{ Serial.print("setting");
  Serial.println(set_temp, DEC);
  Serial.print("switch_interval ");
  Serial.println(switch_interval_for_eeprom, DEC);

  readtemp();

  firsttime();
  if (maintain_breakout == 0)
    maintaintemp_v2();
  maintain_breakout = 0;
}
void maintaintemp_v2()
{ int done = 0;
  int done2 = 0;
  switchcomptime = 0;
  freeze_comptime = 0;
  while (1)
  { Serial.print(F("Free RAM = "));
    Serial.println(freeMemory(), DEC);
    readtemp();
    switchcomp();
    diagnostics();
    //delay(2000);
    //if ((set_temp + 1) <= atoi(sensor1.c_str()) && protection_timer > 30000)
    if (set_temp <= atoi(sensor1.c_str()) && protection_timer > 30000)
    {
      if (done == 0)
      {
        digitalWrite(pump, HIGH);
        //Serial.println("waiting for pump signal");

        pumponcommand = 0;
        while (digitalRead(pump_signal))
        {
          readtemp();
          if (pumponcommand > 30000) //pump fail
          {
            pump_fail = 1;
            shutoff();
            Serial.println("pumpfail");
            break;
          }
        }
        if (pump_fail == 0)
        { //switchcomptime=freeze_comptime;
          //switchcomp();
          Serial.println("pumpon");
          select_fan_comp();
          done = 1;
          done2 = 0;
          protection_timer=0;
        }
        switchcomptime = freeze_comptime;
      }
      //Serial.println("HOT, turning on comp and timer");
      Serial.print("timer");
      Serial.println(switchcomptime);
    }
    //if ((set_temp - 1) > atoi(sensor1.c_str()) && protection_timer > 30000)
     if ( set_temp > atoi(sensor1.c_str()) && protection_timer > 30000)
    {
      if (done2 == 0)
      {

        freeze_comptime = switchcomptime;
        if ((switch_interval - freeze_comptime) < 10000)
        {

          /* code */freeze_comptime = 1;
          if (i == 5)
          {
            /* code */
            i = 6;
          }
          else
          {
            i = 5;
          }
          freeze_comptime = 1;

        }

        digitalWrite(compressorA, LOW);
        digitalWrite(fan1, LOW);
        digitalWrite(compressorB, LOW);
        digitalWrite(fan2, LOW);
        digitalWrite(pump, LOW);


        Serial.println("compressor off");
        Serial.println("fan1off");
        Serial.println("fan2off");
        Serial.println("pumpoff");
        compressor_off_value_sent = 1;
        compressor_a_value_sent = 0;
        compressor_b_value_sent = 0;
        fan_2_value_sent == 0;
        fan_1_value_sent == 0;
        compressor_running = 0;
        fan_is_on = 0;
        donotenterhere = 0;

        protection_timer = 0; //protection timer started
        done2 = 1;
        done = 0;
      }
      switchcomptime = 0;
      //Serial.println("already cold, turning off comp and holding timer");
    }

    if (compressor_running == 1)
    {
      if (i == 6) //check if compressor A is already running
      {
        while (z == 0)
        {
          digitalWrite(fan1, LOW);
          digitalWrite(compressorA, LOW);
          Serial.println("fan1off");
          Serial.println("compressorA off");


          digitalWrite(fan2, HIGH);
          digitalWrite(compressorB, HIGH);
          Serial.println("fan2on");
          Serial.println("compressorB on");
          z = 1;
          y = 0;
        }

      }
      if (i == 5) //check if compressor B is already running
      {
        while (y == 0)
        {
          digitalWrite(fan2, LOW);
          digitalWrite(compressorB, LOW);
          Serial.println("fan2off");
          Serial.println("compressorB off");

          digitalWrite(fan1, HIGH);
          digitalWrite(compressorA, HIGH);
          Serial.println("fan1on");
          Serial.println("compressorA on");
          y = 1;
          z = 0;
        }
      }
      //Serial.println("inside compressor running block");
      //switchcomptime = 0;
    }

    if (Serial.available() > 0)
    {
      incomingByte = Serial.readString();
      if (incomingByte.startsWith("end"))
      {
        shutoff();
        done = 0;
        done2 = 0;
        compressor_a_value_sent = 0;
        compressor_b_value_sent = 0;
        break;
      }
      if (incomingByte.startsWith("settemp"))
      {
        stringexplode();
        set_temp = atoi(tmp);

        //Serial.print("temp is set ");
        //Serial.println(set_temp, DEC);
      }
      if (incomingByte.startsWith("switch_interval"))
      {
        stringexplode();
        switch_interval = atoi(tmp);
        //Serial.println("interval value received switch_interval= "+ switch_interval);
        switch_interval_for_eeprom = switch_interval;
        //Serial.println("switch_interval_for_eeprom = "+ switch_interval_for_eeprom);
        switch_interval = switch_interval * 60 * 1000;
        //Serial.println("converting switch_interval to milliseconds, switch_interval= "+ switch_interval);

      }
    }

  }
}

void select_fan_comp()
{
  if (i == 5)
  {
    digitalWrite(fan1, HIGH);
    digitalWrite(compressorA, HIGH);
    Serial.println("fan1on");
    Serial.println("compressorA on");
    compressor_a_value_sent = 1;    // to remember compressor A is On
    compressor_b_value_sent = 0;

  }
  if (i == 6)
  {
    digitalWrite(fan2, HIGH);
    digitalWrite(compressorB, HIGH);
    Serial.println("fan2on");
    Serial.println("compressorB on");

    compressor_b_value_sent = 1; // to remember compressor B is On
    compressor_a_value_sent = 01;
  }
  compressor_running = 1;
}

void switchcomp()
{ //Serial.println("switching compressors..........switching i value ");
  //Serial.print("printing switching timer value from switch comp block= ");
  //Serial.println(switchcomptime);
  //Serial.print("printing set timer value = ");
  //Serial.println(switch_interval);
  if (switchcomptime > (switch_interval))
  {
    i++;
    switchcomptime = 0;
  }
  if (i == 7)
    i = 5;
  //Serial.print("printing i from switchcomp function, i = ");
  //Serial.println(i);

}
void firsttime()
{
  if (set_temp < atoi(sensor1.c_str()))
  {
    digitalWrite(pump, HIGH);
    //Serial.println("waiting for pump signal");

    pumponcommand = 0;
    while (digitalRead(pump_signal))
    {
      readtemp();
      if (pumponcommand > 30000) //pump fail
      {
        pump_fail = 1;
        shutoff();
        Serial.println("pumpfail");
        break;
      }
    }
    if (pump_fail == 0)
    {
      Serial.println("pumpon");
      pumprelay = 1;
      digitalWrite(fan1, HIGH);
      Serial.println("fan1on");
      digitalWrite(fan2, HIGH);
      Serial.println("fan2on");
      fanoncommand = 0;
      while (fanoncommand < 5000)
      {
        readtemp();
      }
      digitalWrite(compressorA, HIGH);
      digitalWrite(compressorB, HIGH);
      Serial.println("compressorB on");
      Serial.println("compressorA on");

    }
    while (set_temp < atoi(sensor1.c_str()))
    {
      readtemp();
      protection_timer = 0;
      Serial.print(F("Free RAM = "));
      Serial.println(freeMemory(), DEC);
      if (Serial.available() > 0)
      {
        incomingByte = Serial.readString();
        if (incomingByte.startsWith("end"))
        {
          shutoff();

          compressor_a_value_sent = 0;
          compressor_b_value_sent = 0;
          maintain_breakout = 1;
          break;
        }
        if (incomingByte.startsWith("settemp"))
        {
          stringexplode();
          set_temp = atoi(tmp);

          //Serial.print("temp is set ");
          //Serial.println(set_temp, DEC);
        }
        if (incomingByte.startsWith("switch_interval"))
        {
          stringexplode();
          switch_interval = atoi(tmp);
          //Serial.println("interval value received switch_interval= "+ switch_interval);
          switch_interval_for_eeprom = switch_interval;
          //Serial.println("switch_interval_for_eeprom = "+ switch_interval_for_eeprom);
          switch_interval = switch_interval * 60 * 1000;
          //Serial.println("converting switch_interval to milliseconds, switch_interval= "+ switch_interval);

        }
      }
    }
  }

}


void manualmode()
{
  Serial.print("setting");
  Serial.println(set_temp, DEC);
  Serial.print("switch_interval ");
  Serial.println(switch_interval_for_eeprom, DEC);
  while (1)
  { Serial.print(F("Free RAM = "));
    Serial.println(freeMemory(), DEC);
    diagnostics();
    if (Serial.available() > 0)
    {
      incomingByte = Serial.readString();
      if (incomingByte.startsWith("pumpon"))
      {
        digitalWrite(pump, HIGH);
        //Serial.println("waiting for pump signal");

        pumponcommand = 0;
        while (digitalRead(pump_signal))
        {
          readtemp();
          if (pumponcommand > 10000) //pump fail
          {
            pump_fail = 1;
            shutoff();
            Serial.println("pumpfail");
            break;
          }
        }
        if (pump_fail == 0)
          Serial.println("pumpon");
        pumprelay = 1;
      }

      if (incomingByte.startsWith("compressorAON") && pump_fail == 0)
      {
        //digitalWrite(compressorA, HIGH);
        compressorAkey = 1;
        Serial.println("compressorAON");
      }
      if (incomingByte.startsWith("compressorBON") && pump_fail == 0)
      {
        //digitalWrite(compressorB, HIGH);
        compressorBkey = 1;
        Serial.println("compressorBON");
      }
      if (incomingByte.startsWith("fan1on"))
      {
        digitalWrite(fan1, HIGH);
        Serial.println("fan1on");
      }
      if (incomingByte.startsWith("fan2on"))
      {
        digitalWrite(fan2, HIGH);
        Serial.println("fan2on");
      }
      if (incomingByte.startsWith("fan1off"))
      {
        digitalWrite(fan1, LOW);
        Serial.println("fan1off");
      }
      if (incomingByte.startsWith("fan2off"))
      {
        digitalWrite(fan2, LOW);
        Serial.println("fan2off");
      }
      if (incomingByte.startsWith("compressorAoff"))
      {
        //digitalWrite(compressorA, LOW);
        compressorAkey = 0;
        Serial.println("compressorAoff");
      }
      if (incomingByte.startsWith("compressorBoff"))
      {
        //digitalWrite(compressorB, LOW);
        compressorBkey = 0;
        Serial.println("compressorBoff");
      }
      if (incomingByte.startsWith("pumpoff"))
      {
        digitalWrite(pump, LOW);
        Serial.println("pumpoff");
        pumprelay = 0;
      }
      if (incomingByte.startsWith("settemp"))
      {
        stringexplode();
        set_temp = atoi(tmp);

        //Serial.print("temp is set ");
        //Serial.println(set_temp, DEC);
      }
      if (incomingByte.startsWith("switch_interval"))
      {
        stringexplode();
        switch_interval = atoi(tmp);
        //Serial.println("interval value received switch_interval= "+ switch_interval);
        switch_interval_for_eeprom = switch_interval;
        //Serial.println("switch_interval_for_eeprom = "+ switch_interval_for_eeprom);
        switch_interval = switch_interval * 60 * 1000;
        //Serial.println("converting switch_interval to milliseconds, switch_interval= "+ switch_interval);




      }
      if (incomingByte.startsWith("end"))
      {
        shutoff();
        compressorAkey = 0;
        pumprelay = 0;
        Serial.println("compressorAoff");
        compressorBkey = 0;
        Serial.println("compressorBoff");
        Serial.println("clear the popup");
        break;
      }
    }
    //delay(1000); //debug only
    readtemp();
    if (temp_is_set == 1)
    {
      val = digitalRead(pump_signal);

      //if ((set_temp + 1) <= atoi(sensor1.c_str()) && protection_timer > 30000)
      if ( set_temp <= atoi(sensor1.c_str()) && protection_timer > 30000)
      {
        if (compressorAkey == 1 && val == 0)
        {
          digitalWrite(compressorA, HIGH);
          if (compressor_a_value_sent == 0)
          {
            Serial.println("compressorA on");
            compressor_a_value_sent = 1;
            compressor_off_value_sent = 0;
            protection_timer = 0;
          }
        }
        if (compressorBkey == 1 && val == 0)
        {
          digitalWrite(compressorB, HIGH);
          if (compressor_b_value_sent == 0)
          {
            Serial.println("compressorB on");
            compressor_b_value_sent = 1;
            compressor_off_value_sent = 0;
            protection_timer = 0;
          }
        }
        set_protection = 0;
      }
      //if ( ((set_temp - 1) > atoi(sensor1.c_str()) || val == 1) && protection_timer > 30000 )
      if ( (set_temp > atoi(sensor1.c_str()) || val == 1) && protection_timer > 30000 )
      { 
        digitalWrite(compressorA, LOW);

        digitalWrite(compressorB, LOW);

        if (compressor_off_value_sent == 0)
        {
          Serial.println("compressor off");
          compressor_off_value_sent = 1;
          compressor_a_value_sent = 0;
          compressor_b_value_sent = 0;
        }
        if (set_protection == 0)
        {
          protection_timer = 0; //protection timer started
          set_protection = 1;
        }
      }
      if (pumprelay == 1 && val == 1)
      {
        Serial.println("pumpfail");
        compressorAkey = 0;
        Serial.println("compressorAoff");
        compressorBkey = 0;
        Serial.println("compressorBoff");
        shutoff();

      }
    }
  }
}


void stringexplode()
{

  for (int i = 0; i < incomingByte.length(); i++)
  {
    if (flag == 1)
    {
      tmp[tmpCounter] = incomingByte[i];
      tmpCounter++;
    }
    if (incomingByte[i] == ',')
    {
      flag = 1;
    }
  }
  tmp[tmpCounter] = "\0";
  //Serial.println(tmp);
  flag = 0;
  tmpCounter = 0;
}

void shutoff() //turn off all relays
{
  digitalWrite(fan1, LOW);        // connected to Relay 1
  digitalWrite(fan2, LOW);        // connected to Relay 2
  digitalWrite(pump, LOW);        // connected to Relay 3
  digitalWrite(compressorA, LOW); // connected to Relay 4
  digitalWrite(compressorB, LOW); // connected to Relay 4
  Serial.println("compressor off");
  Serial.println("fan1off");
  Serial.println("fan2off");
  Serial.println("pumpoff");

  donotenterhere = 0;
  pump_fail = 0;
  tmpCounter = 0;

  compressorAkey = 0;
  compressorBkey = 0;

  first_time = 0;
  flag = 0;
  pump_is_on = 0;
  i = 5;
  fan_is_on = 0;
  init_comp = 0;
  currently_switching_compressor = 0;
  set_protection = 0;
  compressor_off_value_sent = 0;
  compressor_a_value_sent = 0;
  compressor_b_value_sent = 0;
  fan_1_value_sent = 0;
  fan_2_value_sent = 0;
  high_pressure_A_entry = 0;
  high_pressure_B_entry = 0;
  low_pressure_A_entry = 0;
  low_pressure_B_entry = 0;
  oil_pressure_A_entry = 0;
  oil_pressure_B_entry = 0;

  if (set_temp <= 0)
  {
    negative = 1;
    EEPROM.update(0, set_temp * -1);
  }
  if (set_temp >= 0)
  {
    negative = 0;
    EEPROM.update(0, set_temp);
  }

  EEPROM.update(1, negative);
  EEPROM.update(2, 1); //eeprom stores previous value
  temp_is_set = 1;

  EEPROM.update(5, switch_interval_for_eeprom);   //store minute value
  //Serial.println("actual eeprom timing interval = ");
  //Serial.println(switch_interval_for_eeprom);
  //switch_interval = switch_interval*60*60; //actual switching time in ms



}
void readtemp()
{
  sensors.requestTemperatures(); // Send the command to get temperatures
  sensor1 = String(sensors.getTempCByIndex(0), DEC);
  sensor2 = String(sensors.getTempCByIndex(1), DEC);
  String temp1 = String("A" + sensor1);
  String temp2 = String("B" + sensor2);

  Serial.println(temp1);

  Serial.println(temp2);


}

void diagnostics()
{

  if (!digitalRead(highPA) && high_pressure_A_entry == 0) {
    Serial.println("High Pressure A");
    high_pressure_A_entry = 1;
  }

  if (!digitalRead(highPB) && high_pressure_B_entry == 0) {
    Serial.println("High Pressure B");
    high_pressure_B_entry = 1;
  }

  if (!digitalRead(lowPA) && low_pressure_A_entry == 0) {
    Serial.println("Low Pressure A");
    low_pressure_A_entry == 1;
  }

  if (!digitalRead(lowPB) && low_pressure_B_entry == 0) {
    Serial.println("Low Pressure B");
    low_pressure_B_entry = 1;
  }

  if (!digitalRead(oilA) && oil_pressure_A_entry == 0) {
    Serial.println("Oil Pressure A");
    oil_pressure_A_entry = 1;
  }

  if (!digitalRead(oilB) && oil_pressure_B_entry == 0) {
    Serial.println("Oil Pressure B");
    oil_pressure_B_entry = 1;
  }

}
