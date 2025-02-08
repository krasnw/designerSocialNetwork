<script>
import { editProfileService } from '@/services/editProfile';

export default {
  name: "EditProfilePage",
  data() {
    return {
      userData: {
        username: "",
        phoneNumber: "",
        email: "",
        accessFee: "",
      },
      basicForm: {
        firstName: "",
        lastName: "",
        description: "",
        profileImage: null,
      },
      sensitiveForm: {
        password: "",
        username: "",
        phoneNumber: "",
        email: "",
        newPassword: "",
        newPasswordRepeat: "",
      },
      errors: [],
      showSuccess: false,
      isLoading: true,
      processing: false,
      isDragging: false,
      imagePreview: null,
      passwordFieldTouched: false,
      newPasswordFieldTouched: false,
      repeatPasswordFieldTouched: false,
      emailFieldTouched: false,
      phoneFieldTouched: false,
    };
  },
  computed: {
    isPasswordValid() {
      return !this.passwordFieldTouched || !!this.sensitiveForm.password;
    },
    isNewPasswordValid() {
      if (!this.sensitiveForm.newPassword) return true;
      const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$/;
      return passwordRegex.test(this.sensitiveForm.newPassword);
    },
    isRepeatPasswordValid() {
      if (!this.sensitiveForm.newPasswordRepeat) return true;
      return this.sensitiveForm.newPassword === this.sensitiveForm.newPasswordRepeat;
    },
    newPasswordError() {
      if (!this.newPasswordFieldTouched || this.isNewPasswordValid) return '';
      return 'Hasło musi zawierać minimum 8 znaków, jedną wielką literę, jedną małą literę i jedną cyfrę';
    },
    repeatPasswordError() {
      if (!this.repeatPasswordFieldTouched || this.isRepeatPasswordValid) return '';
      return 'Hasła nie są identyczne';
    },
    currentPasswordError() {
      return !this.isPasswordValid ? 'Wprowadź hasło' : '';
    },
    isEmailValid() {
      if (!this.sensitiveForm.email) return true;
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      return emailRegex.test(this.sensitiveForm.email);
    },
    emailError() {
      if (!this.emailFieldTouched || this.isEmailValid) return '';
      return 'Wprowadź poprawny adres email';
    },
    isPhoneValid() {
      if (!this.sensitiveForm.phoneNumber) return true;
      const cleanPhone = this.sensitiveForm.phoneNumber.replace(/[\s-]/g, '');
      const phoneRegex = /^\+48\d{9}$/;
      return phoneRegex.test(cleanPhone);
    },
    phoneError() {
      if (!this.phoneFieldTouched || this.isPhoneValid) return '';
      return 'Numer telefonu powinien być w formacie: +48000000000';
    },
    hasFormChanges() {
      return this.sensitiveForm.username ||
        this.sensitiveForm.phoneNumber ||
        this.sensitiveForm.email ||
        this.sensitiveForm.newPassword ||
        this.sensitiveForm.accessFee;
    }
  },
  async created() {
    const response = await editProfileService.getUserProfile();
    this.userData = {
      description: response.description,
      username: response.email.split('@')[0],
      phoneNumber: response.phoneNumber,
      email: response.email,
      accessFee: response.accessFee,
    };
    this.basicForm.description = response.description;

    this.isLoading = false;
  },
  methods: {
    hideSuccessMessage() {
      setTimeout(() => {
        this.showSuccess = false;
      }, 5000);
    },
    triggerFileInput() {
      !this.imagePreview && this.$refs.fileInput.click();
    },
    onDragEnter(e) {
      e.preventDefault();
      if (!this.imagePreview) {
        this.isDragging = true;
      }
    },
    onDragLeave(e) {
      e.preventDefault();
      this.isDragging = false;
    },
    onDrop(e) {
      e.preventDefault();
      if (this.imagePreview) return;

      this.isDragging = false;
      const file = e.dataTransfer.files[0];
      this.handleFile(file);
    },
    handleFileUpload(e) {
      const file = e.target.files[0];
      this.handleFile(file);
    },
    handleFile(file) {
      if (!file || !file.type.startsWith('image/')) return;

      const reader = new FileReader();
      reader.onload = (e) => {
        this.imagePreview = e.target.result;
        this.basicForm.profileImage = file;
      };
      reader.readAsDataURL(file);
    },
    removeImage() {
      this.imagePreview = null;
      this.basicForm.profileImage = null;
    },
    validatePassword() {
      this.passwordFieldTouched = true;
    },
    validateNewPassword() {
      this.newPasswordFieldTouched = true;
    },
    validateRepeatPassword() {
      this.repeatPasswordFieldTouched = true;
    },
    validateEmail() {
      this.emailFieldTouched = true;
    },
    validatePhone() {
      this.phoneFieldTouched = true;
    },
    sensitiveFormValid() {
      this.passwordFieldTouched = true;
      this.newPasswordFieldTouched = true;
      this.repeatPasswordFieldTouched = true;
      this.emailFieldTouched = true;
      this.phoneFieldTouched = true;

      if (!this.isPasswordValid) {
        return false;
      }

      if (!this.hasFormChanges) {
        return false;
      }

      if (this.sensitiveForm.newPassword && !this.isNewPasswordValid) {
        return false;
      }

      if (this.sensitiveForm.newPassword && !this.isRepeatPasswordValid) {
        return false;
      }

      if (this.sensitiveForm.email && !this.isEmailValid) {
        return false;
      }

      if (this.sensitiveForm.phoneNumber && !this.isPhoneValid) {
        return false;
      }

      return true;
    },
    async basicFormSubmit() {
      this.processing = true;
      this.errors = [];
      this.showSuccess = false;
      try {
        const formData = new FormData();
        formData.append('FirstName', this.basicForm.firstName.trim());
        formData.append('LastName', this.basicForm.lastName.trim());
        formData.append('Description', this.basicForm.description.trim());
        if (this.basicForm.profileImage) {
          formData.append('ProfileImage', this.basicForm.profileImage);
        } else {
          formData.append('ProfileImage', "");
        }

        await editProfileService.updateBasicUserInfo(formData);
        this.showSuccess = true;
        this.hideSuccessMessage();
      } catch (error) {
        this.errors = error.response.data.errors;
      } finally {
        this.processing = false;
      }
    },
    async sensitiveFormSubmit() {
      this.processing = true;
      this.errors = [];
      this.showSuccess = false;

      if (!this.sensitiveFormValid()) {
        this.processing = false;
        return;
      }

      try {
        const formData = new FormData();
        formData.append('currentPassword', this.sensitiveForm.password);
        formData.append('newPhoneNumber', this.sensitiveForm.phoneNumber.trim());
        formData.append('newEmail', this.sensitiveForm.email.trim());
        formData.append('newAccessFee', this.sensitiveForm.accessFee);
        formData.append('newPassword', this.sensitiveForm.newPassword);

        await editProfileService.updateSensitiveUserInfo(formData);
        this.showSuccess = true;
        this.hideSuccessMessage();
      } catch (error) {
        this.errors = error.response?.data?.errors || ['Wystąpił błąd podczas aktualizacji danych'];
      } finally {
        this.processing = false;
      }
    },
  }
}

</script>

<template>
  <main>
    <h2 class="page-name">Edytuj swoją stronę</h2>
    <div v-if="showSuccess" class="success-message background">
      Dane zostały zaktualizowane
    </div>
    <form class="form background" autocomplete="off" @submit.prevent="basicFormSubmit">
      <h2>Podstawowe informacje</h2>
      <div class="columns">
        <div class="form-group">
          <label for="firstName">Imię</label>
          <input type="text" id="firstName" v-model="basicForm.firstName" autocomplete="new-password" />
        </div>
        <div class="form-group">
          <label for="lastName">Nazwisko</label>
          <input type="text" id="lastName" v-model="basicForm.lastName" />
        </div>
      </div>
      <div class="form-group">
        <label for="description">Opis</label>
        <textarea id="description" v-model="basicForm.description"></textarea>
      </div>
      <div class="form-group">
        <label for="profileImage">Zdjęcie profilowe</label>
        <div class="file-upload" @click="triggerFileInput" @dragenter="onDragEnter" @dragleave="onDragLeave"
          @dragover.prevent @drop="onDrop" :class="{ 'dragging': isDragging }">
          <input type="file" multiple accept="image/*" class="hidden-input" ref="fileInput"
            @change="handleFileUpload" />
          <div v-if="imagePreview" class="preview-container">
            <img :src="imagePreview" alt="Profile preview" class="preview-image" />
            <button type="button" class="remove-button" @click.stop="removeImage">×</button>
          </div>
          <div v-else class="empty-file-upload">
            <span class="icon">+</span>
            <h4 class="text" :class="{ 'error': errors.images }">Dodaj obrazek lub przeciągnij pliki tutaj</h4>
          </div>
        </div>
      </div>
      <button class="accept button" type="submit">Zapisz</button>
    </form>

    <form class="form background" autocomplete="off" @submit.prevent="sensitiveFormSubmit">
      <h2>Dane osobowe</h2>
      <p>Wypełnij tylko te dane które chcesz zmienić</p>
      <h3>Zmiana hasła</h3>
      <div class="form-group">
        <label for="newPassword">Nowe hasło</label>
        <input type="password" id="newPassword" v-model="sensitiveForm.newPassword" autocomplete="new-password"
          :class="{ 'invalid': newPasswordFieldTouched && !isNewPasswordValid }" @blur="validateNewPassword" />
        <span class="error-message" v-if="newPasswordError">{{ newPasswordError }}</span>
      </div>
      <div class="form-group">
        <label for="newPasswordRepeat">Powtórz nowe hasło</label>
        <input type="password" id="newPasswordRepeat" v-model="sensitiveForm.newPasswordRepeat"
          autocomplete="new-password" :class="{ 'invalid': repeatPasswordFieldTouched && !isRepeatPasswordValid }"
          @blur="validateRepeatPassword" />
        <span class="error-message" v-if="repeatPasswordError">{{ repeatPasswordError }}</span>
      </div>
      <h3>Dane użytkownika</h3>
      <div class="form-group">
        <label for="username">Nazwa użytkownika</label>
        <input type="text" id="username" v-model="sensitiveForm.username" autocomplete="off"
          :placeholder="userData.username" />
      </div>
      <div class="form-group">
        <label for="phoneNumber">Numer telefonu</label>
        <input type="text" id="phoneNumber" v-model="sensitiveForm.phoneNumber" autocomplete="off"
          :placeholder="userData.phoneNumber" :class="{ 'invalid': phoneFieldTouched && !isPhoneValid }"
          @blur="validatePhone" />
        <span class="error-message" v-if="phoneError">{{ phoneError }}</span>
      </div>
      <div class="form-group">
        <label for="email">Email</label>
        <input type="email" id="email" v-model="sensitiveForm.email" autocomplete="off" :placeholder="userData.email"
          :class="{ 'invalid': emailFieldTouched && !isEmailValid }" @blur="validateEmail" />
        <span class="error-message" v-if="emailError">{{ emailError }}</span>
      </div>
      <div class="form-group">
        <label for="accessFee">Kwota dostępu</label>
        <input type="number" id="accessFee" v-model="sensitiveForm.accessFee" :placeholder="userData.accessFee" />
      </div>

      <div class="form-group">
        <label for="password">Wprowadź hasło obecne</label>
        <input type="password" id="password" v-model="sensitiveForm.password" autocomplete="new-password"
          :class="{ 'invalid': !isPasswordValid }" @blur="validatePassword" />
        <span class="error-message" v-if="currentPasswordError">{{ currentPasswordError }}</span>
      </div>

      <button class="accept button" type="submit">Zapisz</button>
    </form>

  </main>
</template>

<style scoped>
.hidden-input {
  display: none;
}

.success-message {
  position: fixed;
  top: 5px;
  font-size: 17px;
  padding: 20px;
  font-weight: 700;
  color: var(--text-color);
  width: max-content;
  margin-bottom: 30px;
  z-index: 100;
}

.form {
  display: flex;
  flex-direction: column;
  gap: 15px;
  padding: 20px;
  box-shadow: 5px 5px 25px var(--shadow-color);
  width: 500px;
  max-width: 1200px;
  margin-bottom: 30px;
}

.columns {
  display: flex;
  flex-direction: row;
  gap: 15px;
  width: 100%;
}

.form-group {
  display: flex;
  flex-direction: column;
  width: 100%;
  gap: 7px;

  label {
    font-weight: 700;
    font-size: 14px;
    color: var(--text-color);
  }
}

.file-upload {
  border: 1px dashed var(--element-border-light-color);
  background-color: var(--element-light-color);
  border-radius: 10px;
  text-align: center;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: border-color 0.3s ease-in-out;
  cursor: pointer;
  box-shadow: 5px 5px 25px var(--shadow-color);
  display: flex;
  padding: 20px;
  min-height: 200px;
}

.file-upload:hover {
  background-color: var(--element-hover-light-color);
}

.empty-file-upload {
  flex-direction: column;
  justify-content: center;
  align-items: center;

  .icon {
    font-size: 40px;
    color: var(--placeholder-color);
  }

  .text {
    color: var(--placeholder-color);
    font-weight: 700;
    font-size: 14px;
    max-width: 160px;
  }
}

.dragging {
  border-color: var(--primary-color);
  background-color: var(--element-hover-light-color);
}

.preview-container {
  position: relative;
  width: 100%;
  height: 100%;
}

.preview-image {
  width: 100%;
  height: 100%;
  object-fit: contain;
  border-radius: 8px;
}

.remove-button {
  position: absolute;
  top: 20px;
  right: 20px;
  width: 24px;
  height: 24px;
  border-radius: 50%;
  background: #0009;
  color: white;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 16px;
}

.remove-button:hover {
  background: var(--error-dark-color);
}

.accept {
  background-color: var(--element-light-color);
  border-color: var(--element-border-light-color);
}

input.invalid {
  border-color: var(--delete-button-border-color);
}

input.invalid:focus {
  border-color: var(--delete-button-border-color);
}

.error-message {
  color: var(--error-dark-color);
  font-size: 12px;
  margin-top: 4px;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }

  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>