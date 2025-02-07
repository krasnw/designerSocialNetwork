<script>
import { userService } from '@/services/user';

const MULTIPLIER = 0.1;

export default {
  name: "BuyRubies",
  data() {
    return {
      error: null,
      isActive: false,
      paymentMethod: '',
      blikCode: '',
      blikParts: ['', ''],
      cardNumber: '',
      cardExpirationDate: '',
      cardCvc: '',
      cardHolder: '',
      rubiesAmount: '',
      amount: 0,
      page: 1,
      isValidForm: false,
      cardParts: ['', '', '', ''],
      cardMonth: '',
      cardYear: '',
    };
  },
  computed: {
    canProceed() {
      return this.paymentMethod && this.rubiesAmount > 0;
    },
    isCardValid() {
      return this.cardParts.every(part => part.length === 4)
        && this.cardMonth.length === 2
        && this.cardYear.length === 2
        && this.inRange(this.cardCvc.length, 3, 4)
        && this.cardHolder.length > 0;
    },
    isBlikValid() {
      return this.blikParts.every(part => part.length === 3) &&
        this.blikParts.every(part => /^\d+$/.test(part));
    },
    showInactiveStyles() {
      return this.page === 1 ? false : !this.paymentMethod;
    },
    totalAmount() {
      return this.rubiesAmount ? (parseInt(this.rubiesAmount) * MULTIPLIER).toFixed(2) : 0;
    }
  },
  watch: {
    cardParts: {
      handler(newVal) {
        this.cardNumber = newVal.join('');
      },
      deep: true
    },
    'cardMonth': function (val) {
      if (val.length === 2) this.$refs.yearInput.focus();
    },
    'cardYear': function (val) {
      if (val.length === 2) this.$refs.cvcInput.focus();
    },
    blikParts: {
      handler(newVal) {
        this.blikCode = newVal.join('');
      },
      deep: true
    },
  },
  methods: {
    selectPayment(method) {
      this.paymentMethod = method;
    },
    inRange(value, min, max) {
      return value >= min && value <= max;
    },
    handlePayment() {
      if ((this.paymentMethod === 'card' && this.isCardValid) ||
        (this.paymentMethod === 'blik' && this.isBlikValid)) {
        this.page = 3;
      }
    },
    handleCardInput(index, event) {
      const value = event.target.value;
      if (value.length === 4 && index < 3) {
        const nextInput = this.$refs[`cardPart${index + 1}`][0];
        nextInput?.focus();
      }
    },
    handleKeyDown(index, event) {
      // Handle backspace
      if (event.key === 'Backspace' && !this.cardParts[index] && index > 0) {
        const prevInput = this.$refs[`cardPart${index - 1}`][0];
        prevInput?.focus();
      }
    },
    handleDateKeyDown(event) {
      // Handle backspace for month field
      if (event.key === 'Backspace' && !this.cardMonth && this.$refs.yearInput === document.activeElement) {
        const monthInput = event.target.previousElementSibling;
        monthInput?.focus();
      }
    },
    formatCardMonth(event) {
      let value = event.target.value;
      if (value.length === 1 && parseInt(value) > 1) {
        this.cardMonth = `0${value}`;
        this.$refs.yearInput?.focus();
      }
    },
    handleBlikInput(index, event) {
      const value = event.target.value;
      if (value.length === 3 && index === 0) {
        this.$refs[`blikPart1`][0]?.focus();
      }
    },
    handleBlikKeyDown(index, event) {
      if (event.key === 'Backspace' && !this.blikParts[index] && index > 0) {
        const prevInput = this.$refs[`blikPart${index - 1}`][0];
        prevInput?.focus();
      }
    },
    async handlePayment() {
      try {
        const userData = await userService.getMyData();
        const username = userData.username;
        const formData = new FormData();
        formData.append('amount', this.rubiesAmount);
        formData.append('username', username);
        const response = await userService.buyRubies(formData);
        this.page = 3;
      } catch (error) {
        this.error = error.response.data;
      }
    }
  }
}
</script>

<template>
  <main>
    <h2 class="page-name">Kupuj rubiny</h2>
    <section class="page background">
      <transition name="fade" mode="out-in">
        <article class="option-section" v-if="page === 1" key="page1">
          <div class="cell">
            <span class="row">
              <p>Wpisz ile rubinów podrebujesz</p>
              <input type="number" v-model="rubiesAmount" min="0" class="no-spinners" />
            </span>
            <transition name="fade">
              <p v-if="rubiesAmount">Do opłaty zostanie {{ totalAmount }} zł</p>
            </transition>
          </div>
          <transition name="fade">
            <div class="cell" v-if="rubiesAmount">
              <p>Wybierz metodę płatności</p>
              <div class="payment-method">
                <span class="payment-method__item"
                  :class="{ 'payment-method__item--active': paymentMethod === 'card' || paymentMethod === '' }"
                  @click="selectPayment('card')">
                  💳 Karta płatnicza
                </span>
                <span class="payment-method__item"
                  :class="{ 'payment-method__item--active': paymentMethod === 'blik' || paymentMethod === '' }"
                  @click="selectPayment('blik')">
                  🏦 Kod blik
                </span>
              </div>
            </div>
          </transition>
          <button @click="page++" class="accept-button button" :disabled="!canProceed">
            Dalej
          </button>
        </article>

        <article class="option-section" v-else-if="page === 2 && paymentMethod === 'card'" key="page2card">
          <h3>Dane płatnicze</h3>
          <div class="row-bank">
            <span>
              <p>Wprowadź dane karty bankowej</p>
              <div class="card">
                <div class="card__front">
                  <div class="card__chip"></div>
                  <div class="card__data">
                    <div class="card__number-group">
                      <input v-for="(part, index) in cardParts" :key="index" type="text" v-model="cardParts[index]"
                        :ref="'cardPart' + index" maxlength="4" :placeholder="'0000'" class="card__number-part"
                        @input="handleCardInput(index, $event)" @keydown="handleKeyDown(index, $event)" />
                    </div>
                    <div class="card__info">
                      <div class="card__info-item">
                        <span>Właściciel karty</span>
                        <input type="text" v-model="cardHolder" placeholder="IMIĘ NAZWISKO" class="card__holder" />
                      </div>
                      <div class="card__info-item">
                        <span>Ważność</span>
                        <div class="card__date">
                          <input type="text" v-model="cardMonth" placeholder="MM" maxlength="2" class="card__date-part"
                            @input="formatCardMonth" />
                          <span class="card__date-separator">/</span>
                          <input type="text" v-model="cardYear" ref="yearInput" placeholder="RR" maxlength="2"
                            class="card__date-part" @keydown="handleDateKeyDown" />
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </span>
            <span>
              <p class="cvc-hint">
                Wprowadź kod CVC z tyłu karty
              </p>
              <input type="text" v-model="cardCvc" ref="cvcInput" placeholder="CVC" maxlength="4" class="cvc-input" />
            </span>
          </div>
          <button @click="handlePayment" class="accept-button button" :disabled="!isCardValid">
            Zapłać
          </button>
        </article>

        <article class="option-section" v-else-if="page === 2 && paymentMethod === 'blik'" key="page2blik">
          <h3>Wprowadź kod BLIK</h3>
          <div class="blik-input-container">
            <input v-for="(part, index) in blikParts" :key="index" type="text" v-model="blikParts[index]"
              :ref="'blikPart' + index" maxlength="3" :placeholder="'123'" class="blik-input-part"
              @input="handleBlikInput(index, $event)" @keydown="handleBlikKeyDown(index, $event)" />
          </div>
          <button @click="handlePayment" class="accept-button button" :disabled="!isBlikValid">
            Zapłać
          </button>
        </article>

        <article class="option-section" v-else-if="page === 3" key="page3">
          <h3>Płatność zakończona pomyślnie!</h3>
          <p>Dziękujemy za zakup. Rubiny zostały dodane do Twojego <span class="blue-gradient"
              @click="$router.push('/profile/me')">konta</span>.</p>
          <p @click="page = 1">Kup więcej <span class="green-gradient" style="cursor: pointer;">rubinów</span></p>
        </article>
      </transition>
    </section>
  </main>
</template>

<style scoped>
.page {
  padding: 15px;
  width: max-content;
  transition: all 0.3s ease;
}

.option-section {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.cell {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 20px;
}

.payment-method {
  display: flex;
  gap: 10px;
  align-items: center;
  width: max-content;
  padding: 10px;
  background-color: var(--element-light-color);
  border-radius: 16px;
  border: 0.5px solid var(--element-border-light-color);
}

.payment-method__item {
  cursor: pointer;
  background-color: var(--element-light-color);
  padding: 8px 16px;
  border-radius: 8px;
  border: 0.5px solid var(--element-border-light-color);
  margin: none;
  transition: all 0.3s ease;
}

.payment-method__item--active {
  background-color: var(--element-light-color);
  color: white;
}

.payment-method__item:not(.payment-method__item--active) {
  filter: grayscale(1);
  opacity: 0.7;
}

button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

input {
  padding: 8px;
  border-radius: 8px;
  border: 1px solid var(--element-border-light-color);
}

/* Hide number input spinners */
.no-spinners::-webkit-outer-spin-button,
.no-spinners::-webkit-inner-spin-button {
  -webkit-appearance: none;
  margin: 0;
}

.no-spinners {
  width: 70px;
  -moz-appearance: textfield;
  appearance: textfield;
}

input[type="number"] {
  -moz-appearance: textfield;
  appearance: textfield;
}

/* Animation styles */
.fade-enter-active,
.fade-leave-active {
  transition: all 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
  transform: translateY(20px);
}

.fade-enter-to,
.fade-leave-from {
  opacity: 1;
  transform: translateY(0);
}

.card {
  width: 400px;
  height: 250px;
  perspective: 1000px;
  margin: 20px auto;
}

.row-bank {
  display: flex;
  justify-content: space-around;
  align-items: center;
  gap: 20px;
  padding-left: 20px;
}

.cvc-hint {
  width: 170px;
  text-wrap: balance;
  padding-bottom: 40px;
}

.card__front {
  width: 100%;
  height: 100%;
  background: var(--element-light-color);
  border: 0.5px solid var(--element-border-light-color);
  border-radius: 16px;
  padding: 25px;
  box-shadow: 0 6px 12px rgba(0, 0, 0, 0.15);
  display: flex;
  flex-direction: column;
  gap: 30px;
}

.card__chip {
  width: 50px;
  height: 40px;
  background: linear-gradient(135deg, #ffd700, #b8860b);
  border-radius: 8px;
}

.card__data {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.card__number-group {
  display: flex;
  justify-content: space-around;
  gap: 10px;
  width: 100%;
}

.card__number-part {
  background: transparent;
  border: none;
  color: white;
  font-size: 1.5em;
  width: 75px;
  text-align: center;
  letter-spacing: 1px;
  font-family: var(--bank-card-font);
  box-shadow: none;
}

.card__number-part::placeholder {
  font-family: var(--bank-card-font);
}

.card__info {
  display: flex;
  justify-content: space-between;
  color: white;
}

.card__info-item {
  display: flex;
  flex-direction: column;
  gap: 5px;
}

.card__info-item span {
  font-size: 0.8em;
  text-transform: uppercase;
  opacity: 0.8;
}

.card__holder,
.card__expiry {
  background: transparent;
  border: none;
  color: white;
  font-size: 1.1em;
  font-family: var(--bank-card-font);
  box-shadow: none;
}

.card__holder {
  width: 250px;
}

.card__expiry {
  width: 80px;
}

.card input::placeholder {
  color: rgba(255, 255, 255, 0.5);
}

.card input:focus {
  outline: none;
}

.cvc-input {
  width: 80px;
  margin: 0 auto;
  text-align: center;
}

/* Override default input styles for card inputs */
.card input {
  padding: 4px;
  border: none;
  border-radius: 4px;
}

.card input:focus {
  background: rgba(255, 255, 255, 0.1);
}

.card__date {
  display: flex;
  align-items: center;
  gap: 4px;
}

.card__date-part {
  background: transparent;
  border: none;
  color: white;
  font-size: 1.1em;
  width: 30px;
  text-align: center;
  font-family: var(--bank-card-font);
  box-shadow: none;
}

.card__date-separator {
  color: white;
  font-size: 1.1em;
  font-family: var(--bank-card-font);
  font-weight: 700;
}

/* Ensure all card inputs only accept numbers */
.card__number-part::-webkit-inner-spin-button,
.card__date-part::-webkit-inner-spin-button {
  -webkit-appearance: none;
}

.card__number-part,
.card__date-part {
  -moz-appearance: textfield;
  appearance: textfield;
}

.card input::placeholder {
  color: rgba(255, 255, 255, 0.3);
}

.blik-input-container {
  display: flex;
  gap: 16px;
}

.blik-input-part {
  width: 80px;
  height: 50px;
  text-align: center;
  font-size: 1.5em;
  letter-spacing: 2px;
  border-radius: 8px;
  border: 1px solid var(--element-border-light-color);
  background-color: var(--element-light-color);
  color: white;
}

.blik-input-part:focus {
  outline: none;
  border-color: var(--accent-color);
  box-shadow: 0 0 0 2px rgba(var(--accent-color-rgb), 0.2);
}

.blik-input-part::placeholder {
  color: rgba(255, 255, 255, 0.3);
  letter-spacing: normal;
}
</style>