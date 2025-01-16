<script>
import { ref, onMounted } from 'vue';
import { editProfileService } from '@/services/editProfile';

export default {

  setup() {
    const profileData = ref({
      email: '',
      password: '',
      firstName: '',
      lastName: '',
      phoneNumber: '',
      description: '',
      profileImage: '',
      accessFee: 0
    });

    const loading = ref(false);
    const error = ref('');
    const success = ref('');

    const loadProfile = async () => {
      try {
        loading.value = true;
        profileData.value = await editProfileService.getUserProfile();
      } catch (err) {
        error.value = err.message;
      } finally {
        loading.value = false;
      }
    };

    const updateProfile = async () => {
      try {
        loading.value = true;
        await editProfileService.updateUserProfile(profileData.value);
        error.value = '';
        success.value = 'Profil został zaktualizowany';
      } catch (err) {
        error.value = err.message;
        success.value = '';
      } finally {
        loading.value = false;
      }
    };

    onMounted(() => {
      loadProfile();
    });

    return { profileData, loading, error, success, updateProfile };
  }
}
</script>

<template>
  <main>
    <h2 class="page-name">Edytuj swoją stronę</h2>
    <div v-if="error" class="error background">{{ error }}</div>
    <div v-if="success" class="success background">{{ success }}</div>
    <form class="form-group background" v-if="!loading" @submit.prevent="updateProfile">

      <input v-model="profileData.email" type="email" placeholder="Email" />
      <input v-model="profileData.password" type="password" placeholder="Hasło" />
      <input v-model="profileData.firstName" type="text" placeholder="Imię" />
      <input v-model="profileData.lastName" type="text" placeholder="Nazwisko" />
      <input v-model="profileData.phoneNumber" type="text" placeholder="Telefon" />
      <textarea v-model="profileData.description" placeholder="Opis"></textarea>
      <input v-model="profileData.profileImage" type="text" placeholder="URL zdjęcia" />
      <input v-model.number="profileData.accessFee" type="number" placeholder="Opłata za dostęp" />
      <button class="button" type="submit">Zapisz zmiany</button>
    </form>
    <div v-else>Ładowanie...</div>
  </main>
</template>

<style scoped>
.form-group {
  display: flex;
  flex-direction: column;
  max-width: 400px;
  padding: 20px;
  gap: 15px;
  margin-bottom: 50px;
}

input,
textarea {
  margin: 0;
}

button {
  background-color: var(--element-light-color);
}

.error {
  color: red;
  padding: 20px;
  font-size: 16px;
  color: var(--info-text-color);
  width: max-content;
  font-weight: 700;
  margin-bottom: 20px;
}

.success {
  padding: 20px;
  font-size: 16px;
  color: #84eb87;
  width: max-content;
  font-weight: 700;
  margin-bottom: 20px;
}
</style>