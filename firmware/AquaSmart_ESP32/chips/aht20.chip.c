#include "wokwi-api.h"
#include <stdlib.h>

typedef struct {
  uint32_t attr_temp;
  uint32_t attr_hum;
  uint8_t tx_buf[6];
  uint8_t tx_idx;
  uint8_t last_cmd;
} chip_state_t;

static void update_measurement(chip_state_t *chip) {
  float temp = attr_read_float(chip->attr_temp);
  float hum = attr_read_float(chip->attr_hum);

  if (hum < 0.0f) hum = 0.0f;
  if (hum > 100.0f) hum = 100.0f;
  if (temp < -50.0f) temp = -50.0f;
  if (temp > 150.0f) temp = 150.0f;

  uint32_t raw_h = (uint32_t)((hum / 100.0f) * 1048576.0f);
  if (raw_h > 0xFFFFF) raw_h = 0xFFFFF;

  uint32_t raw_t = (uint32_t)(((temp + 50.0f) / 200.0f) * 1048576.0f);
  if (raw_t > 0xFFFFF) raw_t = 0xFFFFF;

  // Bit 3 (0x08) = Calibrated, Bit 4 (0x10) = Mode/Normal, Bit 7 (0x80) = 0 (Idle)
  chip->tx_buf[0] = 0x18;
  chip->tx_buf[1] = (uint8_t)((raw_h >> 12) & 0xFF);
  chip->tx_buf[2] = (uint8_t)((raw_h >> 4) & 0xFF);
  chip->tx_buf[3] = (uint8_t)(((raw_h & 0x0F) << 4) | ((raw_t >> 16) & 0x0F));
  chip->tx_buf[4] = (uint8_t)((raw_t >> 8) & 0xFF);
  chip->tx_buf[5] = (uint8_t)(raw_t & 0xFF);
}

static bool on_i2c_connect(void *user_data, uint32_t address, bool read) {
  (void)address;
  chip_state_t *chip = (chip_state_t *)user_data;
  if (read) {
    chip->tx_idx = 0;
    update_measurement(chip);
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
  chip_state_t *chip = (chip_state_t *)user_data;
  chip->last_cmd = data;
  return true;
}

static void on_i2c_disconnect(void *user_data) {
  (void)user_data;
}

void chip_init(void) {
  chip_state_t *chip = (chip_state_t *)calloc(1, sizeof(chip_state_t));

  chip->attr_temp = attr_init_float("temperature", 24.5f);
  chip->attr_hum = attr_init_float("humidity", 55.0f);
  update_measurement(chip);

  const i2c_config_t i2c_config = {
    .user_data = chip,
    .address = 0x38,
    .scl = pin_init("SCL", INPUT_PULLUP),
    .sda = pin_init("SDA", INPUT_PULLUP),
    .connect = on_i2c_connect,
    .read = on_i2c_read,
    .write = on_i2c_write,
    .disconnect = on_i2c_disconnect,
  };
  i2c_init(&i2c_config);
}
