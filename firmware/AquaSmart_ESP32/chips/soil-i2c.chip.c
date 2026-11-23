#include "wokwi-api.h"
#include <stdlib.h>

typedef struct {
  uint32_t attr_raw;
  uint8_t tx_buf[2];
  uint8_t tx_idx;
} chip_state_t;

static bool on_i2c_connect(void *user_data, uint32_t address, bool read) {
  (void)address;
  chip_state_t *chip = (chip_state_t *)user_data;
  if (read) {
    uint32_t raw = attr_read(chip->attr_raw);
    if (raw > 0xFFFF) raw = 0xFFFF;
    chip->tx_buf[0] = (uint8_t)((raw >> 8) & 0xFF);
    chip->tx_buf[1] = (uint8_t)(raw & 0xFF);
    chip->tx_idx = 0;
  }
  return true;
}

static uint8_t on_i2c_read(void *user_data) {
  chip_state_t *chip = (chip_state_t *)user_data;
  if (chip->tx_idx < sizeof(chip->tx_buf)) {
    return chip->tx_buf[chip->tx_idx++];
  }
  return 0x00;
}

static bool on_i2c_write(void *user_data, uint8_t data) {
  (void)user_data;
  (void)data;
  return true;
}

static void on_i2c_disconnect(void *user_data) {
  (void)user_data;
}

void chip_init(void) {
  chip_state_t *chip = (chip_state_t *)calloc(1, sizeof(chip_state_t));
  chip->attr_raw = attr_init("moistureRaw", 2200);

  const i2c_config_t i2c_config = {
    .user_data = chip,
    .address = 0x36,
    .scl = pin_init("SCL", INPUT_PULLUP),
    .sda = pin_init("SDA", INPUT_PULLUP),
    .connect = on_i2c_connect,
    .read = on_i2c_read,
    .write = on_i2c_write,
    .disconnect = on_i2c_disconnect,
  };
  i2c_init(&i2c_config);
}
