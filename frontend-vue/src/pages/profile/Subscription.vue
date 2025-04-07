<script>
import RubyIcon from '@/assets/Icons/RubyIcon.vue';
import Spinner from '@/assets/Icons/Spinner.vue';
import { subscribeService } from '@/services/subscription';

export default {
  name: 'Subscription',
  components: {
    RubyIcon,
    Spinner
  },
  data() {
    return {
      isLoading: true,
      subscription: {
        status: false,
        price: 0
      },
      errorMessage: ''
    }
  },
  async mounted() {
    const statusResp = await subscribeService.getSubscriptionState(this.$route.params.username).catch(() => {
      this.$router.push('/error/404');
      return false;
    });
    this.subscription.status = statusResp.isSubscribed
    const priceResp = await subscribeService.getAccessFee(this.$route.params.username).catch(() => {
      this.$router.push('/error/404');
      return false;
    });
    this.subscription.price = priceResp.fee;
    this.isLoading = false;
  },
  methods: {
    async handleSubscription() {
      if (this.subscription.status) {
        await subscribeService.cancelSubscription(this.$route.params.username).catch((err) => {
          this.$router.push(`/error/${err.status}`);
        });
        this.subscription.status = false;
      } else {
        try {
          await subscribeService.subscribe(this.$route.params.username);
          this.subscription.status = true;
          this.errorMessage = '';
        } catch (err) {
          if (err.status === 400 && err.data === 'Insufficient funds') {
            this.errorMessage = 'You do not have enough funds in your account';
            return;
          }
          this.$router.push(`/error/${err.status}`);
        }
      }
    }
  }
}
</script>

<template>
  <main>
    <h2 class="page-name">Subscription Details</h2>
    <div v-if="isLoading" class="loading">Loading
      <Spinner class="spinner" />
    </div>
    <section v-else class="subscription background">
      <div class="subscription__info">
        <h3 class="subscription__title">Subscription Status</h3>
        <p v-if="subscription.status" class="subscription__description">Access is <span
            class="green-gradient">purchased</span></p>
        <p v-else class="subscription__description"><span class="red-gradient">No access</span> to paid content</p>
      </div>

      <div class="subscription__info-row">
        <div class="subscription__info">
          <h3 class="subscription__title">Price</h3>
          <p class="subscription__description">
            Per month: <span class="blue-gradient">{{ subscription.price }}</span>
            <RubyIcon />
          </p>
        </div>
        <div class="subscription__button">
          <button @click="handleSubscription" v-if="subscription.status" class="delete-button button">Cancel
            Subscription</button>
          <button @click="handleSubscription" v-else class="accept-button button">Subscribe</button>
        </div>
      </div>
    </section>
    <p v-if="errorMessage" class="error-message background">{{ errorMessage }}</p>
  </main>
</template>

<style scoped>
.subscription {
  display: flex;
  flex-direction: column;
  gap: 15px;
  padding: 20px;
  width: 500px;
}

.subscription__info-row {
  display: flex;
  justify-content: space-between;
}

.subscription__info {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.subscription__description {
  display: flex;
  align-items: center;
  gap: 5px;
  font-weight: 600;
}

.subscription__button {
  display: flex;
  gap: 10px;
  justify-content: flex-end;
  align-self: flex-end;
}

.loading {
  font-size: 14px;
  font-weight: 600;
  color: var(--info-text-color);
  background-color: var(--element-dark-color);
  border: 0.5px solid var(--element-light-color);
  padding: 20px;
  border-radius: 10px;
  backdrop-filter: blur(10px);
  display: flex;
  gap: 15px;
  flex-direction: row;
  justify-content: center;
  align-items: center;
  width: max-content;
}

.error-message {
  padding: 10px 20px;
  margin-top: 10px;
  font-weight: 600;
  width: max-content;
}
</style>