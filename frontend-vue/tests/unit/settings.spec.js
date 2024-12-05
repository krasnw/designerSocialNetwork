import { shallowMount } from "@vue/test-utils";
import Settings from "@/pages/settings/Settings.vue";

describe("Settings.vue", () => {
  let wrapper;

  beforeEach(() => {
    // Mock localStorage
    Storage.prototype.getItem = jest.fn();
    Storage.prototype.setItem = jest.fn();
    wrapper = shallowMount(Settings);
  });

  describe("validateAndSavePages", () => {
    it("should set isInvalidPages to true when value is less than 3", () => {
      wrapper.vm.postsPerRequest = 2;
      wrapper.vm.validateAndSavePages();

      expect(wrapper.vm.isInvalidPages).toBe(true);
      expect(localStorage.setItem).not.toHaveBeenCalled();
    });

    it("should set isInvalidPages to true when value is greater than 15", () => {
      wrapper.vm.postsPerRequest = 16;
      wrapper.vm.validateAndSavePages();

      expect(wrapper.vm.isInvalidPages).toBe(true);
      expect(localStorage.setItem).not.toHaveBeenCalled();
    });

    it("should save valid value to localStorage and set isInvalidPages to false", () => {
      wrapper.vm.postsPerRequest = 10;
      wrapper.vm.validateAndSavePages();

      expect(wrapper.vm.isInvalidPages).toBe(false);
      expect(localStorage.setItem).toHaveBeenCalledWith("postsPerRequest", 10);
    });

    it("should handle edge cases (3 and 15) as valid values", () => {
      wrapper.vm.postsPerRequest = 3;
      wrapper.vm.validateAndSavePages();
      expect(wrapper.vm.isInvalidPages).toBe(false);
      expect(localStorage.setItem).toHaveBeenCalledWith("postsPerRequest", 3);

      wrapper.vm.postsPerRequest = 15;
      wrapper.vm.validateAndSavePages();
      expect(wrapper.vm.isInvalidPages).toBe(false);
      expect(localStorage.setItem).toHaveBeenCalledWith("postsPerRequest", 15);
    });
  });
});
